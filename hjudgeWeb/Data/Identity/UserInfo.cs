using Microsoft.AspNetCore.Identity;
using System;

namespace hjudgeWeb.Data.Identity
{
    public class OtherUserInfo
    {
        [ItemName("学院")]
        public string Institute { get; set; }

        [ItemName("专业")]
        public string Major { get; set; }

        [ItemName("年级")]
        public int Grade { get; set; }

        [ItemName("个性签名")]
        public string Signature { get; set; }
    }

    public class ItemNameAttribute : Attribute
    {
        public ItemNameAttribute(string v)
        {
            ItemName = v;
        }

        public string ItemName { get; }
    }

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