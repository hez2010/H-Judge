using System;

namespace hjudgeWeb.Models.Problem
{
    public class ProblemDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public int Level { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Type { get; set; }
        public bool Hidden { get; set; }
        public bool IsSucceeded { get; set; }
        public string ErrorMessage { get; set; }
    }
}
