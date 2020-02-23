using System.Collections.Generic;

namespace hjudge.WebHost.Models.Contest
{

    public class ContestListQueryModel
    {
        public class ContestFilter
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// 状态筛选：0 - 未开始, 1 - 进行中, 2 - 已结束
            /// </summary>
            public List<int> Status { get; set; } = new List<int>();
        }
        /// <summary>
        /// 跳过的比赛数量
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 起始查询 Id，用于将 Skip 查询变换为 Where 查询，仅非 0 时生效
        /// </summary>
        public int StartId { get; set; }
        /// <summary>
        /// 需要的比赛数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 查询结果是否包含总数量
        /// </summary>
        public bool RequireTotalCount { get; set; }
        /// <summary>
        /// 非 0 则查询指定小组中的比赛
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// 筛选器
        /// </summary>
        public ContestFilter Filter { get; set; } = new ContestFilter();
    }

}
