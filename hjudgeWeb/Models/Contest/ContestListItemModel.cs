using System;

namespace hjudgeWeb.Models.Contest
{
    public class ContestListItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime RawStartTime { get; set; }
        public string StartTime => $"{RawStartTime.ToShortDateString()} {RawStartTime.ToLongTimeString()}";
        public DateTime RawEndTime { get; set; }
        public string EndTime => $"{RawEndTime.ToShortDateString()} {RawEndTime.ToLongTimeString()}";
        public int ProblemCount { get; set; }
        public bool Hidden { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
