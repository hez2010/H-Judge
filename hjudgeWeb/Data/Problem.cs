using System;

namespace hjudgeWeb.Data
{
    public class Problem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public int Level { get; set; }
        public string Config { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public bool Hidden { get; set; }
    }
}