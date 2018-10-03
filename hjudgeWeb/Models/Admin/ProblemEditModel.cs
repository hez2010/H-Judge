using hjudgeWeb.Configurations;

namespace hjudgeWeb.Models.Admin
{
    public class ProblemEditModel : ResultModel
    {
        public ProblemEditModel()
        {
            Config = new ProblemConfiguration();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public ProblemConfiguration Config { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }
    }
}
