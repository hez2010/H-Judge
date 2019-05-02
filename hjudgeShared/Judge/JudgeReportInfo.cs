using hjudgeCore;

namespace hjudgeShared.Judge
{
    public class JudgeReportInfo
    {
        public enum ReportType
        {
            PreJudge, PostJudge
        }
        public int JudgeId { get; set; }
        public ReportType Type { get; set; }
        public JudgeResult? JudgeResult { get; set; }
    }
}
