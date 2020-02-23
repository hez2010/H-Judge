using System;

namespace hjudge.WebHost.Models.Rank
{
    public class RankContestItemModel
    {
        public bool Accepted { get; set; }
        /// <summary>
        /// 用时
        /// </summary>
        public TimeSpan Time { get; set; }
        /// <summary>
        /// 罚时
        /// </summary>
        public int Penalty { get; set; }
        public float Score { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
    }
}
