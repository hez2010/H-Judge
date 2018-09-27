using System;

namespace hjudgeWeb.Models
{
    public class ProblemListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public int Level { get; set; }
        public int Status { get; set; }
    }
}
