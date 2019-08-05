using hjudge.WebHost.Configurations;
using System.Collections.Generic;

namespace hjudge.WebHost.Models.Problem
{
    public class ProblemEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; }
        public List<string> Languages { get; set; } = new List<string>();
        public int Status { get; set; }
        public bool Hidden { get; set; }
        public ProblemConfig Config { get; set; } = new ProblemConfig();
    }
}
