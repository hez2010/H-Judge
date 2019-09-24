namespace hjudge.WebHost.Data
{
    public class GroupContestConfig
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ContestId { get; set; }

#nullable disable
        public virtual Contest Contest { get; set; }
        public virtual Group Group { get; set; }
#nullable enable
    }
}
