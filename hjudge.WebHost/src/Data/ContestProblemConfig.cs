using System.Collections.Generic;

namespace hjudge.WebHost.Data
{
    public partial class ContestProblemConfig
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public int ProblemId { get; set; }
        public int AcceptCount { get; set; }
        public int SubmissionCount { get; set; }

#nullable disable
        public virtual Contest Contest { get; set; }
        public virtual Problem Problem { get; set; }
#nullable enable
    }
}
