namespace hjudge.WebHost.Models.Problem
{
    public class ProblemQueryModel
    {
        public int ProblemId { get; set; }
        /// <summary>
        /// 查询比赛中的题目，为 0 忽略
        /// </summary>
        public int ContestId { get; set; }
        /// <summary>
        /// 查询小组中的题目，为 0 忽略
        /// </summary>
        public int GroupId { get; set; }
    }
}
