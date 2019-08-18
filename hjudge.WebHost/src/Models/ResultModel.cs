using System.Threading.Tasks;
using hjudge.WebHost.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace hjudge.WebHost.Models
{
    public class ResultModel
    {
        private ErrorDescription errorCode;
        private bool succeeded = true;

        public bool Succeeded
        {
            get => succeeded;
            set
            {
                succeeded = value;
                if (value)
                {
                    errorCode = 0;
                    ErrorMessage = string.Empty;
                }
            }
        }
        public ErrorDescription ErrorCode
        {
            get => errorCode;
            set
            {
                errorCode = value;
                if (errorCode != 0)
                {
                    succeeded = false;
                    ErrorMessage = value.GetDescription();
                }
            }
        }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
