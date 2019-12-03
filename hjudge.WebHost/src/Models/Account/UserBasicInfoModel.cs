namespace hjudge.WebHost.Models.Account
{
    public class UserBasicInfoModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
