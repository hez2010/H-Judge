using System.Collections.Generic;

namespace hjudgeWebHost.Data
{
    public partial class MessageContent
    {
        public MessageContent()
        {
            Messages = new HashSet<Message>();
        }
        public int Id { get; set; }
        public string Content { get; set; }
        
        public ICollection<Message> Messages { get; set; }
    }
}
