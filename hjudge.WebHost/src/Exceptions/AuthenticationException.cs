using System.Net;

namespace hjudge.WebHost.Exceptions
{
    public class AuthenticationException : InterfaceException
    {
        public AuthenticationException(string errorMessage = "") : base(HttpStatusCode.Unauthorized, errorMessage) { }
    }
}
