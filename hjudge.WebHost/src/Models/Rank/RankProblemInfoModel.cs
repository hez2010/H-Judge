namespace hjudge.WebHost.Models.Rank
{
    public class RankProblemInfoModel
    {
        public string ProblemName { get; set; } = string.Empty;
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
    }
}