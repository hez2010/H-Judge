using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public class Group
    {
        public Group()
        {
            GroupJoin = new HashSet<GroupJoin>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsPrivate { get; set; }

        public ICollection<GroupJoin> GroupJoin { get; set; }
    }
}
