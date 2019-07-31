using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeFileHost.Data
{
    public partial class FileHostDbContext : DbContext
    {
        public FileHostDbContext(DbContextOptions<FileHostDbContext> options)
            : base(options)
        {
        }

#nullable disable
        public virtual DbSet<FileRecord> Files { get; set; }
#nullable enable

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var changedEntityNames = this.GetChangedEntityNames();

            this.ChangeTracker.AutoDetectChangesEnabled = false;
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            this.ChangeTracker.AutoDetectChangesEnabled = true;

            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

            foreach (var i in this.ChangeTracker.Entries())
            {
                i.State = EntityState.Detached;
            }
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var changedEntityNames = this.GetChangedEntityNames();

            this.ChangeTracker.AutoDetectChangesEnabled = false;
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            this.ChangeTracker.AutoDetectChangesEnabled = true;

            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

            foreach (var i in this.ChangeTracker.Entries())
            {
                i.State = EntityState.Detached;
            }
            return result;
        }
    }
}
