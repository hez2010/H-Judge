using System.Collections.Generic;
using System.Linq;
using hjudge.Core;

namespace hjudge.WebHost.Configurations
{
    public class ProblemConfig
    {
        /// <summary>
        /// 自定义比较器文件名
        /// </summary>
        public string SpecialJudge { get; set; } = string.Empty;
        /// <summary>
        /// 输入文件名
        /// </summary>
        public string InputFileName { get; set; } = string.Empty;
        /// <summary>
        /// 输出文件名
        /// </summary>
        public string OutputFileName { get; set; } = string.Empty;
        // For older version compatibility
        /// <summary>
        /// 提交文件名，仅用作旧版题目配置向后兼容
        /// </summary>
        public string SubmitFileName { get; set; } = string.Empty;
        /// <summary>
        /// 需要提交的文件名列表
        /// </summary>
        public List<string> SourceFiles { get; set; } = new List<string>();
        /// <summary>
        /// 评测时需要拷贝的额外文件的列表
        /// </summary>
        public List<string> ExtraFiles { get; set; } = new List<string>();
        /// <summary>
        /// 评测数据点，用于提交代码题
        /// </summary>
        public List<DataPoint> Points { get; set; } = new List<DataPoint>();
        /// <summary>
        /// 答案点，用于提交答案题
        /// </summary>
        public AnswerPoint Answer { get; set; } = new AnswerPoint();
        /// <summary>
        /// 答案或输出的比较方法，用于默认比较器
        /// </summary>
        public ComparingOptions ComparingOptions { get; set; } = new ComparingOptions();
        /// <summary>
        /// 使用标准 IO
        /// </summary>
        public bool UseStdIO { get; set; } = true;
        /// <summary>
        /// 编译参数，格式：[语言名称]参数，一行一个
        /// </summary>
        public string CompileArgs { get; set; } = string.Empty;
        /// <summary>
        /// 题目支持的语言，多语言使用 ; 分隔
        /// </summary>
        public string Languages { get; set; } = string.Empty;
        public string ExtraFilesText => ExtraFiles.Aggregate(string.Empty, (accu, next) => accu + next + "\n");
        /// <summary>
        /// 提交内容大小限制，单位：字节
        /// </summary>
        public long CodeSizeLimit { get; set; }
    }
}
