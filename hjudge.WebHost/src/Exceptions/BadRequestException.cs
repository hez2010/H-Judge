namespace hjudge.WebHost.Exceptions
{
    public class BadRequestException : InterfaceException
    {
        public BadRequestException(string errorMessage = "参数不正确") : base(System.Net.HttpStatusCode.BadRequest, errorMessage) { }
    }
}
