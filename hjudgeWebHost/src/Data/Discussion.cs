using hjudgeWebHost.Data.Identity;
using System;

namespace hjudgeWebHost.Data
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
        public UserInfo UserInfo { get; set; }
        public Contest Contest { get; set; }
        public Group Group { get; set; }
        public Problem Problem { get; set; }
#nullable enable
    }
}
