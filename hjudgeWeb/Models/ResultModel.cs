namespace hjudgeWeb.Models
{
    public class ResultModel
    {
        /// <summary>
        /// Indicated whether the request is successed
        /// </summary>
        public bool IsSucceeded { get; set; }
        /// <summary>
        /// If not, this field contains the error messages
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
