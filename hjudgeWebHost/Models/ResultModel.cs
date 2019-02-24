using hjudgeWebHost.Extensions;

namespace hjudgeWebHost.Models
{
    public class ResultModel
    {
        private ErrorDescription errorCode;

        public bool Succeeded { get; set; }
        public ErrorDescription ErrorCode
        {
            get => errorCode;
            set
            {
                errorCode = value;
                ErrorMessage = value.GetDescription();
            }
        }
        public string ErrorMessage { get; set; }
    }
}
