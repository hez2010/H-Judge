using System.Collections.Generic;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemDataUploadModel
    {
        /// <summary>
        /// 上传失败的文件列表
        /// </summary>
        public List<string> FailedFiles { get; } = new List<string>();
    }
}
