using System.Collections.Generic;
using static hjudgeWebHost.Data.Identity.IdentityHelper;

namespace hjudgeWebHost.Models.Account
{
    public class UserInfoModel : ResultModel
    {
        public bool SignedIn { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int Privilege { get; set; }
        public long Coins { get; set; }
        public long Experience { get; set; }
        public List<OtherUserInfoModel>? OtherInfo { get; set; }
    }
}
