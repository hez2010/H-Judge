namespace hjudgeCore
{
    public class ComparingOption
    {
        public ComparingOption()
        {
            IgnoreLineTailWhiteSpaces = IgnoreTextTailLineFeeds = true;
        }

        public bool IgnoreLineTailWhiteSpaces { get; set; }
        public bool IgnoreTextTailLineFeeds { get; set; }
    }
}
