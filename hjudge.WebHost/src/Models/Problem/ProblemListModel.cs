using System.Collections.Generic;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemListModel : ResultModel
    {
        public List<ProblemListItemModel>? Problems { get; set; }
        public int TotalCount { get; set; }
    }
}