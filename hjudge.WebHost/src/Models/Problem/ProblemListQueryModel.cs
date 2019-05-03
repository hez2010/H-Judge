namespace hjudge.WebHost.Models.Problem
{
    public class ProblemListQueryModel
    {
        public class ProblemFilter
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            public int[] Status { get; set; } = new[] { 0, 1, 2 };
        }
        public int Start { get; set; }
        public int StartId { get; set; }
        public int Count { get; set; }
        public bool RequireTotalCount { get; set; }
        public int ContestId { get; set; }
        public int GroupId { get; set; }
        public ProblemFilter Filter { get; set; } = new ProblemFilter();
    }
}
