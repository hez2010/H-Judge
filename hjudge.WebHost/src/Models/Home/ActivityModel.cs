using System;

namespace hjudge.WebHost.Models.Home
{
    public class ActivityModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }
}
