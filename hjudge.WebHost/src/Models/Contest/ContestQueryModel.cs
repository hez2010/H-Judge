namespace hjudge.WebHost.Models.Contest
{
    public class ContestQueryModel
    {
        public int ContestId { get; set; }
        /// <summary>
        /// 查询小组中的比赛，为 0 忽略
        /// </summary>
        public int GroupId { get; set; }
    }
}
