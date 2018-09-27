using System;

namespace hjudgeWeb.Data
{
    public partial class GroupJoin
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? GroupId { get; set; }
        public DateTime JoinTime { get; set; }

        public Group Group { get; set; }
    }
}
