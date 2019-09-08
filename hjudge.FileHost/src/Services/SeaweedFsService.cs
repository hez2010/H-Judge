using Microsoft.Extensions.Options;
using smartbox.SeaweedFs.Client;
using smartbox.SeaweedFs.Client.Core.File;
using System;
using EFSecondLevelCache.Core;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using hjudge.FileHost.Data;

namespace hjudge.FileHost.Services
{
    public class SeaweedFsOptions
    {
        public string MasterHostName { get; set; } = "localhost";
        public int Port { get; set; } = 9333;
    }
    public class SeaweedFsService : IAsyncDisposable
    {
        private readonly MasterConnection connection;
        private readonly FileHostDbContext dbContext;

        public SeaweedFsService(FileHostDbContext dbContext, IOptions<SeaweedFsOptions> options)
        {
            this.dbContext = dbContext;
            connection = new MasterConnection(options.Value.MasterHostName, options.Value.Port);
        }

        public async Task UploadAsync(string fileName, Stream content)
        {
            await connection.Start();
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            FileHandleStatus result;
            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash)/*.Cacheable()*/.FirstOrDefaultAsync();
            var length = content.Length;
            if (fileRecord != null)
            {
                if (await template.CheckFileExists(fileRecord.FileId)) result = await template.UpdateFileByStream(fileRecord.FileId, hash, content);
                else result = await template.SaveFileByStream(hash, content);
                fileRecord.ContentType = string.IsNullOrEmpty(result.ContentType) ? "application/octet-stream" : result.ContentType;
                fileRecord.FileSize = length;
                fileRecord.LastModified = DateTime.Now;
                fileRecord.FileName = hash;
                fileRecord.OriginalFileName = fileName;
                dbContext.Files.Update(fileRecord);
            }
            else
            {
                result = await template.SaveFileByStream(hash, content);

                await dbContext.Files.AddAsync(new FileRecord
                {
                    ContentType = string.IsNullOrEmpty(result.ContentType) ? "application/octet-stream" : result.ContentType,
                    FileSize = length,
                    LastModified = DateTime.Now,
                    FileName = hash,
                    FileId = result.FileId,
                    OriginalFileName = fileName
                });
            }
        }

        public async Task<bool> DeleteAsync(string fileName)
        {
            await connection.Start();
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash)/*.Cacheable()*/.FirstOrDefaultAsync();
            if (fileRecord == null) return true;
            var result = await template.DeleteFile(fileRecord.FileId);
            if (result)
            {
                dbContext.Files.Remove(fileRecord);
            }
            return result;
        }

        public async Task<Stream?> DownloadAsync(string fileName)
        {
            await connection.Start();
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash)/*.Cacheable()*/.FirstOrDefaultAsync();
            if (fileRecord == null) return null;
            var response = await template.GetFileStream(fileRecord.FileId);
            return response.StatusCode == System.Net.HttpStatusCode.OK ? response.OutputStream : null;
        }

        public async Task<List<FileInformation>> ListAsync(string prefix)
        {
            await connection.Start();
            var files = await dbContext.Files.Where(i => i.OriginalFileName.StartsWith(prefix))
                .Select(i => new FileInformation { FileName = i.OriginalFileName, LastModified = i.LastModified.Ticks })
                /*.Cacheable()*/.ToListAsync();
            return files;
        }

        public async Task<List<FileInformation>> ListAsync(IEnumerable<string> fileNames)
        {
            await connection.Start();
            var files = await dbContext.Files.Where(i => fileNames.Contains(i.OriginalFileName))
                .Select(i => new FileInformation { FileName = i.OriginalFileName, LastModified = i.LastModified.Ticks })
                /*.Cacheable()*/.ToListAsync();
            return files;
        }

        private string GetFileNameHash(string str)
        {
            if (string.IsNullOrEmpty(str)) return "default";
            using var cy = SHA256.Create();
            var hash = cy.ComputeHash(Encoding.UTF8.GetBytes(str));
            return hash.Select(i => i.ToString("x2")).Aggregate((accu, next) => accu + next);
        }

        public async ValueTask DisposeAsync()
        {
            await connection.Stop();
            connection.Dispose();
        }
    }
}
