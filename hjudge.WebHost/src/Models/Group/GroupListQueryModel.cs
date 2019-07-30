using System.Collections.Generic;

namespace hjudge.WebHost.Models.Group
{
    public class GroupListQueryModel
    {
        public class GroupFilter
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            public List<int> Status { get; set; } = new List<int>();
        }
        public int Start { get; set; }
        public int StartId { get; set; }
        public int Count { get; set; }
        public bool RequireTotalCount { get; set; }
        public GroupFilter Filter { get; set; } = new GroupFilter();
    }
}
