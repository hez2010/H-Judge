using hjudgeCore;

namespace hjudgeShared.Judge
{
    public class JudgeInfo
    {
        public enum JudgePriority
        {
            Low, Normal, High
        }
        public int JudgeId { get; set; }
        public JudgePriority Priority { get; set; } = JudgePriority.Normal;
        public BuildOptions? BuildOptions { get; set; }
        public JudgeOptions? JudgeOptions { get; set; }
    }
}
