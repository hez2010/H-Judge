using hjudgeWeb.Configurations;
using System;

namespace hjudgeWeb.Models.Contest
{
    public class ContestDetailsModel : ResultModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RawStartTime { get; set; }
        public string StartTime => $"{RawStartTime.ToShortDateString()} {RawStartTime.ToLongTimeString()}";
        public DateTime RawEndTime { get; set; }
        public string EndTime => $"{RawEndTime.ToShortDateString()} {RawEndTime.ToLongTimeString()}";
        public bool Hidden { get; set; }
        public string Status { get; set; }
        public int ProblemCount { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Started => RawStartTime <= DateTime.Now;
        public int SubmissionLimit { get; set; }
        public ContestType Type { get; set; }
    }
}
