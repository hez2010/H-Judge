namespace hjudge.WebHost.Exceptions
{
    public class ForbiddenException : InterfaceException
    {
        public ForbiddenException(string errorMessage = "无权访问该资源") : base(System.Net.HttpStatusCode.Forbidden, errorMessage) { }
    }
}
