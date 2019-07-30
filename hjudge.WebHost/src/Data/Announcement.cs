using hjudge.WebHost.Data.Identity;
using System;

namespace hjudge.WebHost.Data
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public bool Hidden { get; set; }
        public int? GroupId { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual Group Group { get; set; }
#nullable enable
    }
}
