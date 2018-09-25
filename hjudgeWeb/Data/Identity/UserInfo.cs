using Microsoft.AspNetCore.Identity;

namespace hjudgeWeb.Data.Identity
{
    public class UserInfo : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
        public long Coins { get; set; }
        public long Experience { get; set; }
        public int Privilege { get; set; }
        public byte[] Avatar { get; set; }
        public string OtherInfo { get; set; }
    }
}