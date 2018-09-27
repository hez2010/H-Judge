using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public partial class Group
    {
        public Group()
        {
            GroupContestConfig = new HashSet<GroupContestConfig>();
            GroupJoin = new HashSet<GroupJoin>();
            Judge = new HashSet<Judge>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsPrivate { get; set; }
        public string AdditionalInfo { get; set; }

        public ICollection<GroupContestConfig> GroupContestConfig { get; set; }
        public ICollection<GroupJoin> GroupJoin { get; set; }
        public ICollection<Judge> Judge { get; set; }
    }
}
