using hjudgeWebHost.Data.Identity;
using System;

namespace hjudgeWebHost.Data
{
    public class VotesRecord
    {
        public int Id { get; set; }
        public int? ProblemId { get; set; }
        public int? ContestId { get; set; }
        public int? GroupId { get; set; }
        public string UserId { get; set; }
        public DateTime VoteTime { get; set; }
        public int VoteType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public UserInfo UserInfo { get; set; }
        public Problem Problem { get; set; }
        public Contest Contest { get; set; }
        public Group Group { get; set; }
    }
}
