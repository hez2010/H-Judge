using System;
using System.Collections.Generic;

namespace hjudgeWeb.Models.Message
{
    public class MessageContentModel : ResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime RawSendTime { get; set; }
        public string SendTime => $"{RawSendTime.ToShortDateString()} {RawSendTime.ToLongTimeString()}";
        public int Status { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class MessageItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime RawSendTime { get; set; }
        public string SendTime => $"{RawSendTime.ToShortDateString()} {RawSendTime.ToLongTimeString()}";
        public int Status { get; set; }
        public int? ContentId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 1 -- send, 2 -- receive
        /// </summary>
        public int Direction { get; set; }
    }

    public class MessageListModel : ResultModel
    {
        public MessageListModel()
        {
            Messages = new List<MessageItemModel>();
        }

        public List<MessageItemModel> Messages { get; set; }
    }
}
