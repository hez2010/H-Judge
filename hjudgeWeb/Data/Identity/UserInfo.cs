using Microsoft.AspNetCore.Identity;

namespace hjudgeWeb.Data.Identity
{
    public class UserInfo : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
        public long Coins { get; set; }
        public long Experience { get; set; }

        /// <summary>
        /// 1 管理员 2 教师 3 助教 4 学生/选手 5 黑名单
        /// </summary>
        public int Privilege { get; set; }
        public byte[] Avatar { get; set; }
        public string OtherInfo { get; set; }
    }
}