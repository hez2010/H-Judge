using System;
using hjudge.WebHost.Data.Identity;

namespace hjudge.WebHost.Data
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ContentId { get; set; }
        /// <summary>
        /// 1 -- notification, 2 -- private message
        /// </summary>
        public int Type { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 1 -- unread, 2 -- read
        /// </summary>
        public int Status { get; set; }
        public int ReplyId { get; set; }

#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual MessageContent MessageContent { get; set; }
#nullable enable
    }
}
