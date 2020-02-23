namespace hjudge.WebHost.Models.Vote
{
    public class ContestVoteModel
    {
        public int ContestId { get; set; }
        /// <summary>
        /// 投票类型：1 - 好评，2 - 差评
        /// </summary>
        public int VoteType { get; set; }
    }
}