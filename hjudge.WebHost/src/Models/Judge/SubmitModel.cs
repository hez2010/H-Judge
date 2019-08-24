namespace hjudge.WebHost.Models.Judge
{
    public class SubmitModel
    {
        public int ProblemId { get; set; }
        public int ContestId { get; set; }
        public int GroupId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
