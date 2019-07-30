using System;
using System.ComponentModel.DataAnnotations;

namespace hjudgeFileHost.Data
{
    public class FileRecord
    {
        [Key]
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
    }
}