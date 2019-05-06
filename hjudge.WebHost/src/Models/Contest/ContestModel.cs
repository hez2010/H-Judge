using hjudge.WebHost.Configurations;
using System;

namespace hjudge.WebHost.Models.Contest
{
    public class ContestModel : ResultModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool Hidden { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }
        public ContestConfig? Config { get; set; }
    }
}
