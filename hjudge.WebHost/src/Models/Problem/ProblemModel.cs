using System;
using System.Collections.Generic;
using hjudge.WebHost.Models.Language;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public int Level { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public List<LanguageModel> Languages { get; set; } = new List<LanguageModel>();
        public List<string> Sources { get; set; } = new List<string>();
        public int Status { get; set; }
        public bool Hidden { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }
        public int MyVote { get; set; }
    }
}