using hjudgeWeb.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hjudgeWeb.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<UserInfo>
    {
        public class ExperienceCoins
        {
            public long Experience { get; set; }
            public long Coins { get; set; }
        }

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
        public virtual DbSet<MessageContent> MessageContent { get; set; }
        public virtual DbSet<Problem> Problem { get; set; }
        public virtual DbSet<VotesRecord> VotesRecord { get; set; }
        public virtual DbSet<Discussion> Discussion { get; set; }
        public DbQuery<ExperienceCoins> ExperienceCoinsQuery { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contest>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.EndTime).IsRequired();

                entity.Property(e => e.StartTime).IsRequired();

                entity.Property(e => e.Upvote).HasDefaultValueSql("0");

                entity.Property(e => e.Downvote).HasDefaultValueSql("0");

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Contest)
                    .HasForeignKey(d => d.UserId);
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

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.ContestRegister)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreationTime).IsRequired();

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.UserId);
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

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.GroupJoin)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Judge>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.JudgeTime).IsRequired();

                entity.Property(e => e.JudgeCount).HasDefaultValueSql("0");

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

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Judge)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SendTime).IsRequired();

                entity.HasOne(d => d.MessageContent)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ContentId);

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Message)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Message)
                    .HasForeignKey(d => d.ToUserId);
            });

            modelBuilder.Entity<MessageContent>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.MessageContents)
                    .HasForeignKey(d => d.MessageId);
            });

            modelBuilder.Entity<Problem>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.AcceptCount).HasDefaultValueSql("0");

                entity.Property(e => e.CreationTime).IsRequired();

                entity.Property(e => e.SubmissionCount).HasDefaultValueSql("0");

                entity.Property(e => e.Upvote).HasDefaultValueSql("0");

                entity.Property(e => e.Downvote).HasDefaultValueSql("0");

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Problem)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<VotesRecord>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.VoteType).HasDefaultValueSql("1");

                entity.Property(e => e.VoteTime).IsRequired();

                entity.HasOne(d => d.Problem)
                    .WithMany(p => p.VotesRecord)
                    .HasForeignKey(d => d.ProblemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.VotesRecord)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.VotesRecord)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.VotesRecord)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Discussion>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SubmitTime).IsRequired();

                entity.Property(e => e.ReplyId).HasDefaultValueSql("0");

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.Discussion)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Problem)
                    .WithMany(p => p.Discussion)
                    .HasForeignKey(d => d.ProblemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Discussion)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UserInfo)
                    .WithMany(p => p.Discussion)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
