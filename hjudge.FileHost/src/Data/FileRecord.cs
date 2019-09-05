using System;
using System.ComponentModel.DataAnnotations;

namespace hjudge.FileHost.Data
{
    public class FileRecord
    {
        [Key]
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
    }
}