using hjudge.Core;
using System;

namespace hjudge.WebHost.Models.Rank
{
    public class RankContestItemModel
    {
        public bool Accepted { get; set; }
        public TimeSpan Time { get; set; }
        public int Penalty { get; set; }
        public float Score { get; set; }
    }
}
