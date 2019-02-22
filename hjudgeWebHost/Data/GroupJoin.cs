using hjudgeWebHost.Data.Identity;
using System;

namespace hjudgeWebHost.Data
{
    public partial class GroupJoin
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int GroupId { get; set; }
        public DateTime JoinTime { get; set; }

        public UserInfo UserInfo { get; set; }
        public Group Group { get; set; }
    }
}
