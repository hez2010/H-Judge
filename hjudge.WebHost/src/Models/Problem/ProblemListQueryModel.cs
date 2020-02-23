using System.Collections.Generic;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemListQueryModel
    {
        public class ProblemFilter
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// 状态筛选器：0 - 未尝试，1 - 已尝试，2 - 已通过
            /// </summary>
            public List<int> Status { get; set; } = new List<int>();
        }
        /// <summary>
        /// 跳过的题目数量
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 起始查询 Id，用于将 Skip 查询变换为 Where 查询，仅非 0 时生效
        /// </summary>
        public int StartId { get; set; }
        /// <summary>
        /// 需要的题目数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 查询结果是否包含总数量
        /// </summary>
        public bool RequireTotalCount { get; set; }
        /// <summary>
        /// 查询比赛中的题目，为 0 忽略
        /// </summary>
        public int ContestId { get; set; }
        /// <summary>
        /// 查询小组中的题目，为 0 忽略
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// 筛选器
        /// </summary>
        public ProblemFilter Filter { get; set; } = new ProblemFilter();
    }
}
