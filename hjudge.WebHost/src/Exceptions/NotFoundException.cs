using System.Net;

namespace hjudge.WebHost.Exceptions
{
    public class NotFoundException : InterfaceException
    {
        public NotFoundException(string errorMessage = "请求的资源不存在") : base(HttpStatusCode.NotFound, errorMessage) { }
    }
}
