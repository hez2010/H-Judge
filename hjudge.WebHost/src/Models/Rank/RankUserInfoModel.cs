using System;

namespace hjudge.WebHost.Models.Rank
{
    public class RankUserInfoModel
    {
        public int Rank { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float Score { get; set; }
        public TimeSpan Time { get; set; }
        public int Penalty { get; set; }
    }
}
