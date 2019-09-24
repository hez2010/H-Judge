using System;
using hjudge.WebHost.Data.Identity;

namespace hjudge.WebHost.Data
{
    public class GroupJoin
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public DateTime JoinTime { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual Group Group { get; set; }
#nullable enable
    }
}
