namespace hjudgeWeb.Configurations
{
    public class SystemConfig
    {
        public string Environments;
        public bool CanDiscussion;
    }

    public class SystemConfiguration
    {
        public static SystemConfig Config { get; set; }
    }
}
