using hjudgeWeb.Data.Identity;
using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public partial class MessageContent
    {
        public MessageContent()
        {
            Messages = new HashSet<Message>();
        }
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Content { get; set; }

        public Message Message { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
