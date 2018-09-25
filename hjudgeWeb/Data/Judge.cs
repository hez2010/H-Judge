using System;

namespace hjudgeWeb.Data
{
    public class Judge
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime JudgeTime { get; set; }
        public int ProblemId { get; set; }
        public int? ContestId { get; set; }
        public string Code { get; set; }
        public string Result { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public int ResultType { get; set; }
        public string Language { get; set; }
        public string Logs { get; set; }
    }
}