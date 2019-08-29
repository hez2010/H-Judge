using System.Collections.Generic;

namespace hjudge.WebHost.Models.Home
{
    public class ActivityListModel
    {
        public List<ActivityModel> Activities = new List<ActivityModel>();
        public int TotalCount { get; set; }
    }
}
