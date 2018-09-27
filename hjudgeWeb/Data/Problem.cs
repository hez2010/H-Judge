﻿using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public partial class Problem
    {
        public Problem()
        {
            ContestProblemConfig = new HashSet<ContestProblemConfig>();
            Judge = new HashSet<Judge>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public int Level { get; set; }
        public string Config { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public bool Hidden { get; set; }
        public int? AcceptCount { get; set; }
        public int? SubmissionCount { get; set; }
        public string AdditionalInfo { get; set; }

        public ICollection<ContestProblemConfig> ContestProblemConfig { get; set; }
        public ICollection<Judge> Judge { get; set; }
    }
}
