using System;
using System.Collections.Generic;

namespace hjudgeWebHost.Models.Contest
{
    public class ContestListModel : ResultModel
    {
        public class ContestListItemModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool Hidden { get; set; }
            public int Status { get; set; }
            public int Upvote { get; set; }
            public int Downvote { get; set; }
        }
        public List<ContestListItemModel>? Contests { get; set; }
        public int TotalCount { get; set; }
    }
}
