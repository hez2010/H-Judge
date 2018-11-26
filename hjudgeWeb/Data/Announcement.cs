using hjudgeWeb.Data.Identity;
using System;

namespace hjudgeWeb.Data
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public DateTime PublishTime { get; set; }
        public bool Hidden { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}
