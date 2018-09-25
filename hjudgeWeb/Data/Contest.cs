using System;
using System.Collections.Generic;

namespace hjudgeWeb.Data
{
    public class Contest
    {
        public Contest()
        {
            ContestRegister = new HashSet<ContestRegister>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Config { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public int SpecifyCompetitors { get; set; }
        public bool Hidden { get; set; }
        
        public ICollection<ContestRegister> ContestRegister { get; set; }
    }
}