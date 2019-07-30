using System.ComponentModel.DataAnnotations;

namespace hjudge.WebHost.Models.Account
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
