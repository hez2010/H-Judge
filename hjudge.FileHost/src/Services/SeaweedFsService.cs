using hjudgeFileHost.Data;
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

namespace hjudgeFileHost.Services
{
    public class SeaweedFsOptions
    {
        public string MasterHostName { get; set; } = "localhost";
        public int Port { get; set; } = 9333;
    }
    public class SeaweedFsService : IDisposable
    {
        private readonly MasterConnection connection;
        private readonly FileHostDbContext dbContext;

        public SeaweedFsService(FileHostDbContext dbContext, IOptions<SeaweedFsOptions> options)
        {
            this.dbContext = dbContext;
            connection = new MasterConnection(options.Value.MasterHostName, options.Value.Port);
            connection.Start().Wait();
        }

        public async Task Upload(string fileName, Stream content)
        {
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            FileHandleStatus result;

            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash).Cacheable().FirstOrDefaultAsync();
            var isExists = await template.CheckFileExists(fileRecord.FileId);

            if (fileRecord != null && isExists)
            {
                result = await template.UpdateFileByStream(fileRecord.FileId, hash, content);
            }
            else
            {
                result = await template.SaveFileByStream(GetFileNameHash(fileName), content);
            }

            if (isExists)
            {
                fileRecord.ContentType = result.ContentType;
                fileRecord.FileSize = result.Size;
                fileRecord.LastModified = new DateTime(result.LastModified);
                fileRecord.FileName = hash;
                dbContext.Files.Update(fileRecord);
            }
            else
            {
                await dbContext.Files.AddAsync(new FileRecord
                {
                    FileId = result.FileId,
                    FileName = result.Filename,
                    ContentType = result.ContentType,
                    FileSize = result.Size,
                    LastModified = new DateTime(result.LastModified)
                });
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> Delete(string fileName)
        {
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash).Cacheable().FirstOrDefaultAsync();

            var result = await template.DeleteFile(fileRecord.FileId);
            if (result)
            {
                dbContext.Remove(fileRecord);
                await dbContext.SaveChangesAsync();
            }
            return result;
        }

        public async Task<Stream?> Download(string fileName)
        {
            var template = new OperationsTemplate(connection);
            var hash = GetFileNameHash(fileName);
            var fileRecord = await dbContext.Files.Where(i => i.FileName == hash).Cacheable().FirstOrDefaultAsync();

            var response = await template.GetFileStream(fileRecord.FileId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.OutputStream;
            }
            return null;
        }

        public void Dispose()
        {
            connection.Stop().Wait();
        }

        private string GetFileNameHash(string str)
        {
            string ext;
            try
            {
                ext = Path.GetExtension(str);
            }
            catch
            {
                ext = string.Empty;
            }
            if (string.IsNullOrEmpty(str)) return $"default{ext}";
            var cy = SHA256.Create();
            var hash = cy.ComputeHash(Encoding.UTF8.GetBytes(str));
            return hash.Select(i => i.ToString("x2")).Aggregate((accu, next) => accu + next) + ext;
        }

    }
}
