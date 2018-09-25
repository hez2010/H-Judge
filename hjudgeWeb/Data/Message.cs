using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public class Message
    {
        public Message()
        {
            MessageStatus = new HashSet<MessageStatus>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public string UserId { get; set; }
        public string Targets { get; set; }
        public DateTime SendTime { get; set; }
        
        public ICollection<MessageStatus> MessageStatus { get; set; }
    }
}