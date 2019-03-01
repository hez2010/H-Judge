using System.Collections.Generic;

namespace hjudgeWebHost.Models
{
    public class ProblemListModel : ResultModel
    {
        public class ProblemListItemModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Level { get; set; }
            public bool Hidden { get; set; }
            public int Status { get; set; }
            public int AcceptCount { get; set; }
            public int SubmissionCount { get; set; }
        }
        public List<ProblemListItemModel> Problems { get; set; } = new List<ProblemListItemModel>();
        public int TotalCount { get; set; }
    }
}