namespace hjudge.WebHost.Configurations
{
    public enum ContestType
    {
        /// <summary>
        /// 一般比赛
        /// </summary>
        Generic,
        /// <summary>
        /// 只考虑每个题目最后一次提交的比赛
        /// </summary>
        LastSubmit,
        /// <summary>
        /// 带罚时的比赛
        /// </summary>
        Penalty
    }

    public enum ResultDisplayMode
    {
        /// <summary>
        /// 即时显示
        /// </summary>
        Intime,
        /// <summary>
        /// 比赛结束后显示
        /// </summary>
        AfterContest,
        /// <summary>
        /// 不显示
        /// </summary>
        Never
    }

    public enum ResultDisplayType
    {
        /// <summary>
        /// 显示详情
        /// </summary>
        Detailed,
        /// <summary>
        /// 只显示概要
        /// </summary>
        Summary
    }

    public enum ScoreCountingMode
    {
        /// <summary>
        /// 所有得分
        /// </summary>
        All,
        /// <summary>
        /// 只计算 Accepted 得分
        /// </summary>
        OnlyAccepted
    }
    public class ContestConfig
    {
        /// <summary>
        /// 比赛类型
        /// </summary>
        public ContestType Type { get; set; } = ContestType.Generic;
        /// <summary>
        /// 提交次数限制
        /// </summary>
        public int SubmissionLimit { get; set; }
        /// <summary>
        /// 结果反馈模式
        /// </summary>
        public ResultDisplayMode ResultMode { get; set; } = ResultDisplayMode.Intime;
        /// <summary>
        /// 结果显示模式
        /// </summary>
        public ResultDisplayType ResultType { get; set; } = ResultDisplayType.Detailed;
        /// <summary>
        /// 显示排名
        /// </summary>
        public bool ShowRank { get; set; } = true;
        /// <summary>
        /// 计分模式
        /// </summary>
        public ScoreCountingMode ScoreMode { get; set; } = ScoreCountingMode.All;
        /// <summary>
        /// 比赛结束前 1 小时自动封榜
        /// </summary>
        public bool AutoStopRank { get; set; }
        /// <summary>
        /// 语言，多语言用 ; 分隔
        /// </summary>
        public string Languages { get; set; } = string.Empty;
        /// <summary>
        /// 是否允许学生公开源代码
        /// </summary>
        public bool CanMakeResultPublic { get; set; } = false;
        /// <summary>
        /// 是否允许讨论
        /// </summary>
        public bool CanDiscussion { get; set; } = true;
    }
}
