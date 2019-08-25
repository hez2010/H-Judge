using hjudge.Core;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Models.Statistics
{
    public class StatisticsListModel
    {
        public class StatisticsListItemModel
        {
            public int ProblemId { get; set; }
            public string ProblemName { get; set; } = string.Empty;
            public int? ContestId { get; set; }
            public int? GroupId { get; set; }
            public int ResultId { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public int ResultType { get; set; } = -1;
            public string Result => Enum.GetName(typeof(ResultCode), ResultType)?.Replace("_", " ") ?? "Unknown Error";
            public DateTime Time { get; set; }
        }
        public List<StatisticsListItemModel>? Statistics { get; set; }
        public int TotalCount { get; set; }
    }
}
