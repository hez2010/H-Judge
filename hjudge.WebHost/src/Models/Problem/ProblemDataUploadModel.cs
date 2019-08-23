using hjudge.WebHost.Configurations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemDataUploadModel
    {
        public List<string> FailedFiles { get; } = new List<string>();
    }
}
