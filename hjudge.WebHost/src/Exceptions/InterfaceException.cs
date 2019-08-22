using System;
using System.Net;

namespace hjudge.WebHost.Exceptions
{
    public class InterfaceException : Exception
    {
        public InterfaceException(HttpStatusCode code, string? errorMessage)
        {
            Code = code;
            if (string.IsNullOrEmpty(errorMessage)) Message = GetErrorMessage(code) ?? "UnknownError";
            else Message = errorMessage;
        }

        private static string? GetErrorMessage(HttpStatusCode code) => Enum.GetName(typeof(HttpStatusCode), code);

        public HttpStatusCode Code { get; set; }
        public override string Message { get; }
    }
}
