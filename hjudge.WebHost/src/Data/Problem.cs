using System;
using System.Collections.Generic;
using hjudge.WebHost.Data.Identity;

namespace hjudge.WebHost.Data
{
    public class Problem
    {
        public Problem()
        {
            ContestProblemConfig = new HashSet<ContestProblemConfig>();
            Judge = new HashSet<Judge>();
            VotesRecord = new HashSet<VotesRecord>();
            Discussion = new HashSet<Discussion>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public int Level { get; set; }
        public string Config { get; set; } = string.Empty;
        public int Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool Hidden { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public string? AdditionalInfo { get; set; } = string.Empty;
        public int Upvote { get; set; }
        public int Downvote { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
#nullable enable

        public virtual ICollection<ContestProblemConfig> ContestProblemConfig { get; set; }
        public virtual ICollection<Judge> Judge { get; set; }
        public virtual ICollection<VotesRecord> VotesRecord { get; set; }
        public virtual ICollection<Discussion> Discussion { get; set; }
    }
}
