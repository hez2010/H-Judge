using hjudge.WebHost.Configurations;
using System;
using System.Collections.Generic;

namespace hjudge.WebHost.Models.Contest
{
    public class ContestEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Hidden { get; set; }
        public string? Password { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContestConfig Config { get; set; } = new ContestConfig();
        public List<int> Problems { get; set; } = new List<int>();
    }
}
