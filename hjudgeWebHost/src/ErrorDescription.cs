using System.ComponentModel;

namespace hjudgeWebHost
{
    public enum ErrorDescription : int
    {
        [Description("请求中出现错误")]
        GenericError = 1,
        [Description("没有足够权限")]
        NoEnoughPrivilege = 2,
        [Description("没有登录账户")]
        NotSignedIn = 3,
        [Description("用户不存在")]
        UserNotExist = 4,
        [Description("文件格式错误")]
        FileBadFormat = 5,
        [Description("文件大小超出限制")]
        FileSizeExceeded = 6,
        [Description("参数不正确")]
        ArgumentError = 7,
        [Description("用户信息验证失败")]
        AuthenticationFailed = 8,
        [Description("资源不存在")]
        ResourceNotFound = 404,
        [Description("服务器内部异常")]
        InteralServerException = 500
    }
}
