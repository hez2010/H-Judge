using System.Collections.Generic;

namespace hjudge.WebHost.Models.Rank
{
    public class RankContestStatisticsModel
    {
        public int ContestId { get; set; }
        public int GroupId { get; set; }

        public Dictionary<int, RankProblemInfoModel> ProblemInfos = new Dictionary<int, RankProblemInfoModel>();
        // userId -> userName, name
        public Dictionary<string, RankUserInfoModel> UserInfos = new Dictionary<string, RankUserInfoModel>();
        // userId -> problemId -> rank item
        public Dictionary<string, Dictionary<int, RankContestItemModel>> RankInfos { get; set; } = new Dictionary<string, Dictionary<int, RankContestItemModel>>();
    }
}
