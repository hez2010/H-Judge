using hjudgeWeb.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hjudgeWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserInfo>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contest> Contest { get; set; }
        public DbSet<ContestRegister> ContestRegister { get; set; }
        public DbSet<Judge> Judge { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<MessageStatus> MessageStatus { get; set; }
        public DbSet<Problem> Problem { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<GroupJoin> GroupJoin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contest>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.EndTime).IsRequired();

                entity.Property(e => e.StartTime).IsRequired();
            });

            modelBuilder.Entity<ContestRegister>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.ContestRegister)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Judge>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.JudgeTime).IsRequired();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SendTime).IsRequired();
            });

            modelBuilder.Entity<Problem>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreationTime).IsRequired();
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreationTime).IsRequired();
            });


            modelBuilder.Entity<GroupJoin>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.GroupId).IsRequired();

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupJoin)
                    .HasForeignKey(d => d.GroupId);
            });

            modelBuilder.Entity<MessageStatus>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.MessageId).IsRequired();

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.MessageStatus)
                    .HasForeignKey(d => d.MessageId);
            });
        }
    }
}