using hjudgeWeb.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hjudgeWeb.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<UserInfo>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Contest> Contest { get; set; }
        public virtual DbSet<ContestProblemConfig> ContestProblemConfig { get; set; }
        public virtual DbSet<ContestRegister> ContestRegister { get; set; }
        public virtual DbSet<Group> Group { get; set; }
        public virtual DbSet<GroupContestConfig> GroupContestConfig { get; set; }
        public virtual DbSet<GroupJoin> GroupJoin { get; set; }
        public virtual DbSet<Judge> Judge { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<MessageStatus> MessageStatus { get; set; }
        public virtual DbSet<Problem> Problem { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contest>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.EndTime).IsRequired();

                entity.Property(e => e.StartTime).IsRequired();
            });

            modelBuilder.Entity<ContestProblemConfig>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.AcceptCount).HasDefaultValueSql("0");

                entity.Property(e => e.SubmissionCount).HasDefaultValueSql("0");

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.ContestProblemConfig)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Problem)
                    .WithMany(p => p.ContestProblemConfig)
                    .HasForeignKey(d => d.ProblemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContestRegister>(entity =>
            {
                entity.HasIndex(e => e.ContestId);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.ContestRegister)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreationTime).IsRequired();
            });

            modelBuilder.Entity<GroupContestConfig>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.GroupContestConfig)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupContestConfig)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GroupJoin>(entity =>
            {
                entity.HasIndex(e => e.GroupId);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.JoinTime).IsRequired();

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupJoin)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Judge>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.JudgeTime).IsRequired();

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.Judge)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Judge)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Problem)
                    .WithMany(p => p.Judge)
                    .HasForeignKey(d => d.ProblemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SendTime).IsRequired();
            });

            modelBuilder.Entity<MessageStatus>(entity =>
            {
                entity.HasIndex(e => e.MessageId);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.OperationTime).IsRequired();

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.MessageStatus)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Problem>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.AcceptCount).HasDefaultValueSql("0");

                entity.Property(e => e.CreationTime).IsRequired();

                entity.Property(e => e.SubmissionCount).HasDefaultValueSql("0");
            });
        }
    }
}
