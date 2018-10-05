using System;
using System.Collections.Generic;
using System.Linq;

namespace hjudgeWeb.Models.Contest
{
    public class RankUserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class RankProblemInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AcceptedCount { get; set; }
        public int SubmissionCount { get; set; }
    }

    public class RankSubmitInfo
    {
        public RankSubmitInfo()
        {
            TimeCost = TimeSpan.Zero;
        }

        public float Score { get; set; }
        public TimeSpan TimeCost { get; set; }
        public int SubmissionCount { get; set; }
        public bool IsAccepted { get; set; }
        public int PenaltyCount { get; set; }
    }

    public class SingleUserRankInfo
    {
        public SingleUserRankInfo()
        {
            UserInfo = new RankUserInfo();
            SubmitInfo = new Dictionary<int, RankSubmitInfo>();
        }

        public int Rank { get; set; }
        public RankUserInfo UserInfo { get; set; }
        public float FullScore => SubmitInfo.Values.Sum(i => i.Score);
        public TimeSpan TimeCost
        {
            get
            {
                var result = TimeSpan.Zero;
                foreach (var i in SubmitInfo.Values)
                {
                    result += i.TimeCost + i.PenaltyCount * TimeSpan.FromMinutes(20);
                }
                return result;
            }
        }
        public int Penalty => 20 * SubmitInfo.Values.Sum(i => i.PenaltyCount);
        public Dictionary<int, RankSubmitInfo> SubmitInfo { get; set; }
    }

    public class ContestRankModel : ResultModel
    {
        public ContestRankModel()
        {
            RankInfo = new Dictionary<string, SingleUserRankInfo>();
            ProblemInfo = new Dictionary<int, RankProblemInfo>();
        }

        public Dictionary<string, SingleUserRankInfo> RankInfo { get; set; }
        public Dictionary<int, RankProblemInfo> ProblemInfo { get; set; }
    }
}
