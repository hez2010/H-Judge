using System;
using System.Collections.Generic;

namespace hjudgeWebHost.Models.Group
{
    public class GroupListModel : ResultModel
    {
        public class GroupListItemModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public DateTime CreationTime { get; set; }
            public bool IsPrivate { get; set; }
        }
        public List<GroupListItemModel>? Groups { get; set; }
        public int TotalCount { get; set; }
    }
}
