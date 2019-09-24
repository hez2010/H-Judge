using System.Net;

namespace hjudge.WebHost.Exceptions
{
    public class BadRequestException : InterfaceException
    {
        public BadRequestException(string errorMessage = "参数不正确") : base(HttpStatusCode.BadRequest, errorMessage) { }
    }
}
