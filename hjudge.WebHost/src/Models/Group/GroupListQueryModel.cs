using System.Collections.Generic;

namespace hjudge.WebHost.Models.Group
{
    public class GroupListQueryModel
    {
        public class GroupFilter
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// 状态筛选：0 - 已加入，1 - 未加入
            /// </summary>
            public List<int> Status { get; set; } = new List<int>();
        }
        /// <summary>
        /// 跳过的小组数量
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 起始查询 Id，用于将 Skip 查询变换为 Where 查询，仅非 0 时生效
        /// </summary>
        public int StartId { get; set; }
        /// <summary>
        /// 需要的小组数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 查询结果是否包含总数量
        /// </summary>
        public bool RequireTotalCount { get; set; }
        /// <summary>
        /// 筛选器
        /// </summary>
        public GroupFilter Filter { get; set; } = new GroupFilter();
    }
}
