using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public partial class Contest
    {
        public Contest()
        {
            ContestProblemConfig = new HashSet<ContestProblemConfig>();
            ContestRegister = new HashSet<ContestRegister>();
            GroupContestConfig = new HashSet<GroupContestConfig>();
            Judge = new HashSet<Judge>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Config { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public bool SpecifyCompetitors { get; set; }
        public bool Hidden { get; set; }
        public string AdditionalInfo { get; set; }

        public ICollection<ContestProblemConfig> ContestProblemConfig { get; set; }
        public ICollection<ContestRegister> ContestRegister { get; set; }
        public ICollection<GroupContestConfig> GroupContestConfig { get; set; }
        public ICollection<Judge> Judge { get; set; }
    }
}
