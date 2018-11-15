using System.Collections.Generic;
using static hjudgeWeb.Data.Identity.IdentityHelper;

namespace hjudgeWeb.Models.Account
{
    public class UserInfoModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string Name { get; set; }
        public long Coins { get; set; }
        public long Experience { get; set; }
        public int Privilege { get; set; }
        public List<OtherInfoList> OtherInfo { get; set; }
        public bool IsSignedIn { get; set; }
        public int CoinsBonus { get; set; }
    }
}
