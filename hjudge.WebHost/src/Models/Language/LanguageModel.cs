namespace hjudge.WebHost.Models.Language
{
    public class LanguageModel
    {
        /// <summary>
        /// 语言名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 语言相关信息
        /// </summary>
        public string Information { get; set; } = string.Empty;
        /// <summary>
        /// highlightjs 语法高亮配置文件名称
        /// </summary>
        public string SyntaxHighlight { get; set; } = string.Empty;
    }
}
