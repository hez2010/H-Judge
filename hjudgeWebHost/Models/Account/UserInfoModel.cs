using System.Collections.Generic;
using static hjudgeWebHost.Data.Identity.IdentityHelper;

namespace hjudgeWebHost.Models.Account
{
    public class UserInfoModel : ResultModel
    {
        public bool SignedIn { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int Privilege { get; set; }
        public List<OtherInfoList> OtherInfo { get; set; }
    }
}
