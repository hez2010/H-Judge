namespace hjudge.WebHost.Models.Statistics
{
    public class StatisticsListQueryModel
    {
        public int Start { get; set; }
        public int StartId { get; set; }
        public int Count { get; set; }
        public bool RequireTotalCount { get; set; }
        public int? ProblemId { get; set; }
        public int? ContestId { get; set; }
        public int? GroupId { get; set; }
        public string? UserId { get; set; }
    }
}