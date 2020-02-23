using hjudge.Core;

namespace hjudge.WebHost.Configurations
{
    public class LanguageConfig
    {
        // Generic
        /// <summary>
        /// 语言名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 语言信息
        /// </summary>
        public string Information { get; set; } = string.Empty;
        /// <summary>
        /// 扩展名
        /// </summary>
        public string Extensions { get; set; } = string.Empty;
        /// <summary>
        /// highlightjs 高亮模板配置文件名
        /// </summary>
        public string SyntaxHighlight { get; set; } = string.Empty;
        /// <summary>
        /// 默认禁用
        /// </summary>
        public bool DisabledByDefault { get; set; } = false;
        //Compiler
        /// <summary>
        /// 编译器可执行文件名
        /// </summary>
        public string CompilerExec { get; set; } = string.Empty;
        /// <summary>
        /// 编译器运行参数
        /// </summary>
        public string CompilerArgs { get; set; } = string.Empty;
        /// <summary>
        /// 编译输出问题匹配器，使用正则表达式
        /// </summary>
        public string CompilerProblemMatcher { get; set; } = string.Empty;
        /// <summary>
        /// 编译日志显示格式，可用 $i 匹配 <see cref="CompilerProblemMatcher" /> 的正则匹配结果
        /// </summary>
        public string CompilerDisplayFormat { get; set; } = string.Empty;
        /// <summary>
        /// 编译日志包含标准输出
        /// </summary>
        public bool CompilerReadStdOutput { get; set; }
        /// <summary>
        /// 编译日志包含标准错误
        /// </summary>
        public bool CompilerReadStdError { get; set; } = true;
        //Static check
        /// <summary>
        /// 静态检查器可执行文件名
        /// </summary>
        public string StaticCheckExec { get; set; } = string.Empty;
        /// <summary>
        /// 静态检查器运行参数
        /// </summary>
        public string StaticCheckArgs { get; set; } = string.Empty;
        /// <summary>
        /// 静态检查输出问题匹配器，使用正则表达式
        /// </summary>
        public string StaticCheckProblemMatcher { get; set; } = string.Empty;
        /// <summary>
        /// 静态检查日志显示格式，可用 $i 匹配 <see cref="CompilerProblemMatcher" /> 的正则匹配结果
        /// </summary>
        public string StaticCheckDisplayFormat { get; set; } = string.Empty;
        /// <summary>
        /// 静态检查日志包含标准输出
        /// </summary>
        public bool StaticCheckReadStdOutput { get; set; }
        /// <summary>
        /// 静态检查日志包含标准错误
        /// </summary>
        public bool StaticCheckReadStdError { get; set; } = true;
        //Run option
        /// <summary>
        /// 需要运行的编译后程序文件名
        /// </summary>
        public string RunExec { get; set; } = string.Empty;
        /// <summary>
        /// 运行参数
        /// </summary>
        public string RunArgs { get; set; } = string.Empty;
        /// <summary>
        /// 活跃进程数量限制
        /// </summary>
        public int ActiveProcessLimit { get; set; } = 1;
        /// <summary>
        /// 遇到标准错误输出的处理方式
        /// </summary>
        public StdErrBehavior StandardErrorBehavior { get; set; } = StdErrBehavior.Ignore;
    }
}
