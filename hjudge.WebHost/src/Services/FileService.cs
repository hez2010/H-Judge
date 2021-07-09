using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace hjudge.WebHost.Services
{
    public interface IFileService
    {
        IAsyncEnumerable<DownloadResult> DownloadFilesAsync(IEnumerable<string> files);
        IAsyncEnumerable<UploadResult> UploadFilesAsync(IEnumerable<UploadInfo> files);
        Task<IEnumerable<DeleteResult>> DeleteFilesAsync(IEnumerable<string> files);
        Task<IEnumerable<string>> ListFilesAsync(string prefix);
    }
    public class FileService : Files.FilesClient, IFileService, IAsyncDisposable
    {
        private GrpcChannel? channel;
        private Files.FilesClient? client;
        private readonly IConfiguration configuration;

        public FileService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private void Init()
        {
            if (client != null) return;
            var section = configuration.GetSection("FileServer");
            channel = GrpcChannel.ForAddress($"{section["HostName"]}:{section["Port"]}", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 2147483647,
                MaxSendMessageSize = 150 * 1048576
            });
            client = new Files.FilesClient(channel);
        }

        public async IAsyncEnumerable<DownloadResult> DownloadFilesAsync(IEnumerable<string> files)
        {
            Init();
            var request = new DownloadRequest();
            request.FileNames.AddRange(files);
            var response = client!.DownloadFiles(request);
            while (await response.ResponseStream.MoveNext(default))
            {
                foreach (var i in response.ResponseStream.Current.Result)
                {
                    yield return i;
                }
            }
        }

        public async IAsyncEnumerable<UploadResult> UploadFilesAsync(IEnumerable<UploadInfo> files)
        {
            Init();
            var request = new UploadRequest();
            request.Infos.AddRange(files);
            var response = client!.UploadFiles();
            await response.RequestStream.WriteAsync(request);
            await response.RequestStream.CompleteAsync();
            var result = await response.ResponseAsync;
            foreach (var i in result.Results)
            {
                yield return i;
            }
        }

        public async Task<IEnumerable<DeleteResult>> DeleteFilesAsync(IEnumerable<string> files)
        {
            Init();
            var request = new DeleteRequest();
            request.FileNames.AddRange(files);
            var response = await client!.DeleteFilesAsync(request);
            return response.Results;
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string prefix)
        {
            Init();
            var request = new ListRequest
            {
                Prefix = prefix
            };
            var response = await client!.ListFilesAsync(request);
            return response.FileInfos.Select(i => i.FileName);
        }

        public async ValueTask DisposeAsync()
        {
            if (channel != null)
            {
                try
                {
                    await channel.ShutdownAsync();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}