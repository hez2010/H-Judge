using Microsoft.EntityFrameworkCore;

namespace hjudge.FileHost.Data
{
    public class FileHostDbContext : DbContext
    {
        public FileHostDbContext(DbContextOptions<FileHostDbContext> options)
            : base(options)
        {
        }

#nullable disable
        public virtual DbSet<FileRecord> Files { get; set; }
#nullable enable
    }
}
