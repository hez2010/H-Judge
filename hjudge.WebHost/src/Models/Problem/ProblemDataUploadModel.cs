using System.Collections.Generic;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemDataUploadModel
    {
        public List<string> FailedFiles { get; } = new List<string>();
    }
}
