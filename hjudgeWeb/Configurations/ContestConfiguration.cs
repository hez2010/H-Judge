namespace hjudgeWeb.Configurations
{
    public enum ContestType
    {
        Generic,
        LastSubmit,
        Penalty
    }

    public enum ResultDisplayMode
    {
        Intime,
        AfterContest,
        Never
    }

    public enum ResultDisplayType
    {
        Detailed,
        Summary
    }

    public enum ScoreCountingMode
    {
        All,
        OnlyAccepted
    }

    public class ContestConfiguration
    {
        public ContestConfiguration()
        {
            Type = ContestType.Generic;
            ResultMode = ResultDisplayMode.Intime;
            ResultType = ResultDisplayType.Detailed;
            ScoreMode = ScoreCountingMode.All;
            ShowRank = true;
            CanMakeResultPublic = true;
            CanDisscussion = true;
        }

        public ContestType Type { get; set; }
        public int SubmissionLimit { get; set; }
        public ResultDisplayMode ResultMode { get; set; }
        public ResultDisplayType ResultType { get; set; }
        public bool ShowRank { get; set; }
        public ScoreCountingMode ScoreMode { get; set; }
        public bool AutoStopRank { get; set; }
        public string Languages { get; set; }
        public bool CanMakeResultPublic { get; set; }
        public bool CanDisscussion { get; set; }
    }
}