using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost.Models.Message
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public int Type { get; set; }
    }
}
