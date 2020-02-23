using System.Collections.Generic;
using hjudge.Core;

namespace hjudge.WebHost.Models.Judge
{
    public class SubmitModel
    {
        /// <summary>
        /// 题目 Id
        /// </summary>
        public int ProblemId { get; set; }
        /// <summary>
        /// 比赛 Id，为 0 忽略
        /// </summary>
        public int ContestId { get; set; }
        /// <summary>
        /// 小组 Id，为 0 忽略
        /// </summary>
        public int GroupId { get; set; }
        public string Language { get; set; } = string.Empty;
        public List<Source> Content { get; set; } = new List<Source>();
    }
}
