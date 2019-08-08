using hjudge.WebHost.Data.Identity;
using System;

namespace hjudge.WebHost.Data
{
    public partial class Judge
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime JudgeTime { get; set; }
        public int ProblemId { get; set; }
        public int? ContestId { get; set; }
        public int? GroupId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Result { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Description { get; set; } = string.Empty;
        public int ResultType { get; set; }
        public float FullScore { get; set; }
        public string? Language { get; set; } = string.Empty;
        public string? Logs { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int JudgeCount { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual Contest Contest { get; set; }
        public virtual Group Group { get; set; }
        public virtual Problem Problem { get; set; }
#nullable enable
    }
}
