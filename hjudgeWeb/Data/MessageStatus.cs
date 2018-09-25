using System;

namespace hjudgeWeb.Data
{
    public class MessageStatus
    {
        public int Id { get; set; }
        public DateTime OperationTime { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public int Status { get; set; }
        
        public Message Message { get; set; }
    }
}