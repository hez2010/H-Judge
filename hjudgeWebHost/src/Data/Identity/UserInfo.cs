using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace hjudgeWebHost.Data.Identity
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
        public byte[] Avatar { get; set; } = new byte[0];
        public string? OtherInfo { get; set; }
        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime LastSignedIn { get; set; }
        /// <summary>
        /// 连续登录天数
        /// </summary>
        public int ContinuousSignedIn { get; set; }

        public int AcceptedCount { get; set; }
        public int SubmissionCount { get; set; }
        public int MessageReplyCount { get; set; }


        public ICollection<Judge> Judge { get; set; }
        public ICollection<Problem> Problem { get; set; }
        public ICollection<Contest> Contest { get; set; }
        public ICollection<Group> Group { get; set; }
        public ICollection<ContestRegister> ContestRegister { get; set; }
        public ICollection<GroupJoin> GroupJoin { get; set; }
        public ICollection<Message> Message { get; set; }
        public ICollection<MessageContent> MessageStatus { get; set; }
        public ICollection<VotesRecord> VotesRecord { get; set; }
        public ICollection<Discussion> Discussion { get; set; }
        public ICollection<Announcement> Announcement { get; set; }
    }
}