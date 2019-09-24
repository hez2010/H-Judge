using System.Net;

namespace hjudge.WebHost.Exceptions
{
    public class ForbiddenException : InterfaceException
    {
        public ForbiddenException(string errorMessage = "没有权限访问该资源") : base(HttpStatusCode.Forbidden, errorMessage) { }
    }
}
