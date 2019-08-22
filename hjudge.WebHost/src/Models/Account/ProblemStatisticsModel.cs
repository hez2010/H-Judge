using System.Collections.Generic;

namespace hjudge.WebHost.Models.Account
{
    public class ProblemStatisticsModel
    {
        public List<int> SolvedProblems { get; set; } = new List<int>();
        public List<int> TriedProblems { get; set; } = new List<int>();
    }
}
