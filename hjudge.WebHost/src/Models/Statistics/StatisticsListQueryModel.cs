using hjudge.Core;

namespace hjudge.WebHost.Models.Statistics
{
    public class StatisticsListQueryModel
    {
        /// <summary>
        /// 跳过的提交数量
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 起始查询 Id，用于将 Skip 查询变换为 Where 查询，仅非 0 时生效
        /// </summary>
        public int StartId { get; set; }
        /// <summary>
        /// 需要的提交数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 查询结果是否包含总数量
        /// </summary>
        public bool RequireTotalCount { get; set; }
        /// <summary>
        /// 查询指定题目的提交，0 为查询所有题目中的提交，null 为不包含任何题目中的提交
        /// </summary>
        public int? ProblemId { get; set; }
        /// <summary>
        /// 查询指定比赛的提交，0 为查询所有比赛中的提交，null 为不包含任何比赛中的提交
        /// </summary>
        public int? ContestId { get; set; }
        /// <summary>
        /// 查询指定小组的提交，0 为查询所有小组中的提交，null 为不包含任何小组中的提交
        /// </summary>
        public int? GroupId { get; set; }
        /// <summary>
        /// 查询指定用户名的提交，null 为查询所有用户的提交
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 查询指定结果的提交，null 为查询所有结果的提交，可用值参考 <see cref="ResultCode"/> 的枚举名称
        /// </summary>
        public string? Result { get; set; }
    }
}