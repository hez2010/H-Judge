using hjudge.WebHost.Data.Identity;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Data
{
    public partial class Message
    {
        public Message()
        {
            MessageContents = new HashSet<MessageContent>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? ContentId { get; set; }
        /// <summary>
        /// 1 -- notification, 2 -- private message
        /// </summary>
        public int Type { get; set; }
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 1 -- unread, 2 -- read
        /// </summary>
        public int Status { get; set; }
        public int ReplyId { get; set; }

#nullable disable
        public UserInfo UserInfo { get; set; }
        public MessageContent MessageContent { get; set; }
#nullable enable
        public ICollection<MessageContent> MessageContents { get; set; }
    }
}
