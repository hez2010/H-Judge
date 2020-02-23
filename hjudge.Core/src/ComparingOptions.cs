namespace hjudge.Core
{
    public class ComparingOptions
    {
        /// <summary>
        /// 忽略行末空格
        /// </summary>
        public bool IgnoreLineTailWhiteSpaces { get; set; } = true;
        /// <summary>
        /// 忽略文末空行
        /// </summary>
        public bool IgnoreTextTailLineFeeds { get; set; } = true;
    }
}
