namespace hjudge.WebHost.Models.Judge
{
    public class SubmitModel
    {
        public int ProblemId;
        public int ContestId;
        public int GroupId;
        public string Language = string.Empty;
        public string Content = string.Empty;
    }
}
