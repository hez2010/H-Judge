namespace hjudge.WebHost.Configurations
{
    public enum ContestType : int
    {
        Generic,
        LastSubmit,
        Penalty
    }

    public enum ResultDisplayMode : int
    {
        Intime,
        AfterContest,
        Never
    }

    public enum ResultDisplayType : int
    {
        Detailed,
        Summary
    }

    public enum ScoreCountingMode : int
    {
        All,
        OnlyAccepted
    }
    public class ContestConfig
    {
        public ContestType Type { get; set; } = ContestType.Generic;
        public int SubmissionLimit { get; set; }
        public ResultDisplayMode ResultMode { get; set; } = ResultDisplayMode.Intime;
        public ResultDisplayType ResultType { get; set; } = ResultDisplayType.Detailed;
        public bool ShowRank { get; set; } = true;
        public ScoreCountingMode ScoreMode { get; set; } = ScoreCountingMode.All;
        public bool AutoStopRank { get; set; }
        public string Languages { get; set; } = string.Empty;
        public bool CanMakeResultPublic { get; set; } = false;
        public bool CanDiscussion { get; set; } = true;
    }
}
