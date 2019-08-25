using System.Net;

namespace hjudge.WebHost.Models
{
    public class ErrorModel
    {
        public HttpStatusCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
