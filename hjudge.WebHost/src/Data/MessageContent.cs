using System.Collections.Generic;

namespace hjudge.WebHost.Data
{
    public class MessageContent
    {
        public MessageContent()
        {
            Messages = new HashSet<Message>();
        }
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        
        public virtual ICollection<Message> Messages { get; set; }
    }
}
