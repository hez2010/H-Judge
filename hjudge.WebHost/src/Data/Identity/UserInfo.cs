using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Data.Identity
{
    public class OtherUserInfo
    {
        [ItemName("学号")]
        public string SchoolNumber { get; set; } = string.Empty;

        [ItemName("学院")]
        public string Institute { get; set; } = string.Empty;

        [ItemName("专业")]
        public string Major { get; set; } = string.Empty;

        [ItemName("年级")]
        public int Grade { get; set; }

        [ItemName("个性签名")]
        public string Signature { get; set; } = string.Empty;
    }

    public class ItemNameAttribute : Attribute
    {
        public ItemNameAttribute(string v)
        {
            ItemName = v;
        }

        public string ItemName { get; } = string.Empty;
    }

    public class UserInfo : IdentityUser
    {
        public UserInfo()
        {
            Judge = new HashSet<Judge>();
            Problem = new HashSet<Problem>();
            Contest = new HashSet<Contest>();
            Group = new HashSet<Group>();
            ContestRegister = new HashSet<ContestRegister>();
            GroupJoin = new HashSet<GroupJoin>();
            Message = new HashSet<Message>();
            MessageStatus = new HashSet<MessageContent>();
            VotesRecord = new HashSet<VotesRecord>();
            Discussion = new HashSet<Discussion>();
            Announcement = new HashSet<Announcement>();
        }

        [PersonalData]
        public string Name { get; set; } = string.Empty;
        public long Coins { get; set; }
        public long Experience { get; set; }

        /// <summary>
        /// 1 管理员 2 教师 3 助教 4 学生/选手 5 黑名单
        /// </summary>
        public int Privilege { get; set; }
        public byte[]? Avatar { get; set; }
        public string OtherInfo { get; set; } = string.Empty;
        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime LastSignedIn { get; set; } = DateTime.Parse("1970-01-01T00:00:00");
        /// <summary>
        /// 连续登录天数
        /// </summary>
        public int ContinuousSignedIn { get; set; }

        public int AcceptedCount { get; set; }
        public int SubmissionCount { get; set; }
        public int MessageReplyCount { get; set; }


        public virtual ICollection<Judge> Judge { get; set; }
        public virtual ICollection<Problem> Problem { get; set; }
        public virtual ICollection<Contest> Contest { get; set; }
        public virtual ICollection<Group> Group { get; set; }
        public virtual ICollection<ContestRegister> ContestRegister { get; set; }
        public virtual ICollection<GroupJoin> GroupJoin { get; set; }
        public virtual ICollection<Message> Message { get; set; }
        public virtual ICollection<MessageContent> MessageStatus { get; set; }
        public virtual ICollection<VotesRecord> VotesRecord { get; set; }
        public virtual ICollection<Discussion> Discussion { get; set; }
        public virtual ICollection<Announcement> Announcement { get; set; }
    }
}