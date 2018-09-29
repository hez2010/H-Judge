using System;
using System.Linq;

namespace hjudgeWeb.Models.Problem
{
    public class ProblemDetailsModel : ResultModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RawCreationTime { get; set; }
        public string CreationTime => $"{RawCreationTime.ToShortDateString()} {RawCreationTime.ToLongTimeString()}";
        public int RawLevel { get; set; }
        public string Level => Enumerable.Repeat("⭐", RawLevel).Aggregate((accu, next) => accu + next);
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int RawType { get; set; }
        public string Type => RawType == 1 ? "提交代码" : "提交答案";
        public int RawStatus { get; set; }
        public string Status => RawStatus == 0 ? "未尝试" : RawStatus == 1 ? "已尝试" : "已通过";
        public bool Hidden { get; set; }
    }
}
