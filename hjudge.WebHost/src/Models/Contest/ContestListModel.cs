using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Models.Contest
{
    public class ContestListModel
    {
        public class ContestListItemModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool Hidden { get; set; }
            public int Upvote { get; set; }
            public int Downvote { get; set; }
        }
        public List<ContestListItemModel>? Contests { get; set; }
        public int TotalCount { get; set; }
        public DateTime CurrentTime { get; set; }
    }
}
