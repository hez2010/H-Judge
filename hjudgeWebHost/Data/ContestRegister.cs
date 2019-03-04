using hjudgeWebHost.Data.Identity;

namespace hjudgeWebHost.Data
{
    public partial class ContestRegister
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        
#nullable disable
        public UserInfo UserInfo { get; set; }
        public Contest Contest { get; set; }
#nullable enable
    }
}
