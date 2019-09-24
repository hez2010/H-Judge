using System;
using hjudge.WebHost.Data.Identity;

namespace hjudge.WebHost.Data
{
    public class Discussion
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? ProblemId { get; set; }
        public int? ContestId { get; set; }
        public int? GroupId { get; set; }
        public DateTime SubmitTime { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int ReplyId { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual Contest Contest { get; set; }
        public virtual Group Group { get; set; }
        public virtual Problem Problem { get; set; }
#nullable enable
    }
}
