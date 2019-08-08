namespace hjudge.WebHost.Models.Problem
{
    public class ProblemListItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool Hidden { get; set; }
        public int Status { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }
    }
}