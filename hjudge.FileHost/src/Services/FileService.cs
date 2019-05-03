using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace hjudgeFileHost.Services
{
    public class FileService : Files.FilesBase
    {
        private readonly SeaweedFsService seaweed;

        public FileService(SeaweedFsService seaweed)
        {
            this.seaweed = seaweed;
        }
        public override async Task<UploadResponse> UploadFiles(IAsyncStreamReader<UploadRequest> requestStream, ServerCallContext context)
        {
            var result = new UploadResponse();
            while (await requestStream.MoveNext())
            {
                foreach (var i in requestStream.Current.Info)
                {
                    using var stream = new MemoryStream();
                    i.Content.WriteTo(stream);
                    await seaweed.Upload(i.FileName, stream);

                    result.Result.Add(new UploadResult
                    {
                        FileName = i.FileName,
                        Succeeded = true
                    });
                }
            }
            return result;
        }

        public override async Task DownloadFiles(DownloadRequest request, IServerStreamWriter<DownloadResponse> responseStream, ServerCallContext context)
        {
            var result = new DownloadResponse();
            foreach (var i in request.Info)
            {
                var stream = await seaweed.Download(i.FileName);
                result.Result.Add(new DownloadResult
                {
                    Content = await ByteString.FromStreamAsync(stream),
                    FileName = i.FileName,
                    Succeeded = stream != null
                });
            }
            await responseStream.WriteAsync(result);
        }

        public override async Task<DeleteResponse> DeleteFiles(DeleteRequest request, ServerCallContext context)
        {
            var result = new DeleteResponse();
            foreach (var i in request.Info)
            {
                result.Result.Add(new DeleteResult
                {
                    FileName = i.FileName,
                    Succeeded = await seaweed.Delete(i.FileName)
                });
            }
            return result;
        }
    }
}
