﻿using hjudgeWeb.Data.Identity;
using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public partial class Message
    {
        public Message()
        {
            MessageContents = new HashSet<MessageContent>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? ContentId { get; set; }
        /// <summary>
        /// 1 -- notification, 2 -- private message
        /// </summary>
        public int Type { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 1 -- unread, 2 -- read
        /// </summary>
        public int Status { get; set; }
        public int ReplyId { get; set; }

        public UserInfo UserInfo { get; set; }
        public MessageContent MessageContent { get; set; }
        public ICollection<MessageContent> MessageContents { get; set; }
    }
}