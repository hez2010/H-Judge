using hjudgeWeb.Data.Identity;
using System;

namespace hjudgeWeb.Data
{
    public partial class Judge
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime JudgeTime { get; set; }
        public int ProblemId { get; set; }
        public int ContestId { get; set; }
        public int GroupId { get; set; }
        public string Content { get; set; }
        public string Result { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public int ResultType { get; set; }
        public float FullScore { get; set; }
        public string Language { get; set; }
        public string Logs { get; set; }
        public string AdditionalInfo { get; set; }
        public bool IsPublic { get; set; }
        public int JudgeCount { get; set; }

        public UserInfo UserInfo { get; set; }
        public Contest Contest { get; set; }
        public Group Group { get; set; }
        public Problem Problem { get; set; }
    }
}
