using hjudge.WebHost.Data.Identity;
using System;

namespace hjudge.WebHost.Data
{
    public class VotesRecord
    {
        public int Id { get; set; }
        public int? ProblemId { get; set; }
        public int? ContestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime VoteTime { get; set; }
        public int VoteType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

#nullable disable
        public UserInfo UserInfo { get; set; }
        public Problem Problem { get; set; }
        public Contest Contest { get; set; }
#nullable enable
    }
}
