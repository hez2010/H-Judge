using System.ComponentModel.DataAnnotations;

namespace hjudge.WebHost.Models.Account
{
    public class ResetEmailModel
    {
        public string Email { get; set; } = string.Empty;
    }
    public class ResetModel
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "密码不能为空"), MinLength(6, ErrorMessage = "密码长度最少 6 位")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "确认密码不能为空"), Compare(nameof(Password), ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "验证码不能为空")]
        public string Token { get; set; } = string.Empty;
    }

}
