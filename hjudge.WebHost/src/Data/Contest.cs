using hjudge.WebHost.Data.Identity;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Data
{
    public partial class Contest
    {
        public Contest()
        {
            ContestProblemConfig = new HashSet<ContestProblemConfig>();
            ContestRegister = new HashSet<ContestRegister>();
            GroupContestConfig = new HashSet<GroupContestConfig>();
            Judge = new HashSet<Judge>();
            VotesRecord = new HashSet<VotesRecord>();
            Discussion = new HashSet<Discussion>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Config { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool SpecifyCompetitors { get; set; }
        public bool Hidden { get; set; }
        public string? AdditionalInfo { get; set; } = string.Empty;
        public int Upvote { get; set; }
        public int Downvote { get; set; }
#nullable disable
        public virtual UserInfo UserInfo { get; set; }
#nullable enable

        public virtual ICollection<ContestProblemConfig> ContestProblemConfig { get; set; }
        public virtual ICollection<ContestRegister> ContestRegister { get; set; }
        public virtual ICollection<GroupContestConfig> GroupContestConfig { get; set; }
        public virtual ICollection<Judge> Judge { get; set; }
        public virtual ICollection<VotesRecord> VotesRecord { get; set; }
        public virtual ICollection<Discussion> Discussion { get; set; }
    }
}
