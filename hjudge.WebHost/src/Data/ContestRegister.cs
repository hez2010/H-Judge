using hjudge.WebHost.Data.Identity;

namespace hjudge.WebHost.Data
{
    public partial class ContestRegister
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        
#nullable disable
        public virtual UserInfo UserInfo { get; set; }
        public virtual Contest Contest { get; set; }
#nullable enable
    }
}
