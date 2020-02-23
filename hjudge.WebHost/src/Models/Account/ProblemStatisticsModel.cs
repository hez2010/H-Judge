using System.Collections.Generic;

namespace hjudge.WebHost.Models.Account
{
    public class ProblemStatisticsModel
    {
        /// <summary>
        /// 已解决的问题
        /// </summary>
        public List<int> SolvedProblems { get; set; } = new List<int>();
        /// <summary>
        /// 已尝试的问题
        /// </summary>
        public List<int> TriedProblems { get; set; } = new List<int>();
    }
}
