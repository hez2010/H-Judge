using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using hjudgeFileHost.Data;

namespace hjudgeFileHost.Services
{
    public class FileService : Files.FilesBase
    {
        private readonly SeaweedFsService seaweed;
        private readonly FileHostDbContext dbContext;

        public FileService(SeaweedFsService seaweed, FileHostDbContext dbContext)
        {
            this.seaweed = seaweed;
            this.dbContext = dbContext;
        }
        public override async Task<UploadResponse> UploadFiles(IAsyncStreamReader<UploadRequest> requestStream, ServerCallContext context)
        {
            var result = new UploadResponse();
            while (await requestStream.MoveNext())
            {
                Console.WriteLine($"Count: {requestStream.Current.Infos.Count}");
                foreach (var i in requestStream.Current.Infos)
                {
                    using var stream = new MemoryStream();
                    i.Content.WriteTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        await seaweed.UploadAsync(i.FileName, stream);
                        result.Results.Add(new UploadResult
                        {
                            FileName = i.FileName,
                            Succeeded = true
                        });
                    }
                    catch
                    {
                        result.Results.Add(new UploadResult
                        {
                            FileName = i.FileName,
                            Succeeded = false
                        });
                    }
                }
            }
            await dbContext.SaveChangesAsync();
            return result;
        }

        public override async Task DownloadFiles(DownloadRequest request, IServerStreamWriter<DownloadResponse> responseStream, ServerCallContext context)
        {
            foreach (var i in request.FileNames)
            {
                var result = new DownloadResponse();
                Stream? stream = null;
                try
                {
                    stream = await seaweed.DownloadAsync(i);
                }
                catch { }
                var succeeded = true;
                if (stream != null) stream.Seek(0, SeekOrigin.Begin);
                else { stream = new MemoryStream(); succeeded = false; }
                result.Result.Add(new DownloadResult
                {
                    Content = await ByteString.FromStreamAsync(stream),
                    FileName = i,
                    Succeeded = succeeded
                });
                await responseStream.WriteAsync(result);
                stream?.Dispose();
            }
        }

        public override async Task<DeleteResponse> DeleteFiles(DeleteRequest request, ServerCallContext context)
        {
            var result = new DeleteResponse();
            foreach (var i in request.FileNames)
            {
                bool succeeded = false;
                try
                {
                    succeeded = await seaweed.DeleteAsync(i);
                }
                catch { }
                result.Results.Add(new DeleteResult
                {
                    FileName = i,
                    Succeeded = succeeded
                });
            }
            await dbContext.SaveChangesAsync();
            return result;
        }

        public override async Task<ListResponse> ListFiles(ListRequest request, ServerCallContext context)
        {
            var result = seaweed.ListAsync(request.Prefix);
            var response = new ListResponse();
            response.FileNames.AddRange(await result);
            return response;
        }
    }
}
