using hjudgeWeb.Configurations;
using System;
using System.Collections.Generic;

namespace hjudgeWeb.Models.Admin
{
    public class Competitor
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class ContestEditModel : ResultModel
    {
        public ContestEditModel()
        {
            Competitors = new List<Competitor>();
            Config = new ContestConfiguration();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ContestConfiguration Config { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }
        public string ProblemSet { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Password { get; set; }
        public bool SpecifyCompetitors { get; set; }
        public List<Competitor> Competitors { get; set; }
    }
}
