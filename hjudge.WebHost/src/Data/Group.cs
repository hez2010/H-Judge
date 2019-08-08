using hjudge.WebHost.Data.Identity;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Data
{
    public partial class Group
    {
        public Group()
        {
            GroupContestConfig = new HashSet<GroupContestConfig>();
            GroupJoin = new HashSet<GroupJoin>();
            Judge = new HashSet<Judge>();
            Discussion = new HashSet<Discussion>();
            Announcement = new HashSet<Announcement>();
        }

        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public bool IsPrivate { get; set; }
        public string? AdditionalInfo { get; set; } = string.Empty;

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
#nullable enable

        public virtual ICollection<GroupContestConfig> GroupContestConfig { get; set; }
        public virtual ICollection<GroupJoin> GroupJoin { get; set; }
        public virtual ICollection<Judge> Judge { get; set; }
        public virtual ICollection<Discussion> Discussion { get; set; }
        public virtual ICollection<Announcement> Announcement { get; set; }
    }
}
