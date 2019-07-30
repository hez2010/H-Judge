using System.ComponentModel.DataAnnotations;

namespace hjudge.WebHost.Models.Account
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

}
