namespace hjudgeWeb.Data
{
    public partial class ContestProblemConfig
    {
        public int Id { get; set; }
        public int? ContestId { get; set; }
        public int? ProblemId { get; set; }
        public int? AcceptCount { get; set; }
        public int? SubmissionCount { get; set; }

        public Contest Contest { get; set; }
        public Problem Problem { get; set; }
    }
}
