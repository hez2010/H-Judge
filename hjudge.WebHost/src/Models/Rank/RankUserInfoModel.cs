using System;

namespace hjudge.WebHost.Models.Rank
{
    public class RankUserInfoModel
    {
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float Score { get; set; }
        /// <summary>
        /// 用时
        /// </summary>
        public TimeSpan Time { get; set; }
        /// <summary>
        /// 罚时
        /// </summary>
        public int Penalty { get; set; }
    }
}
