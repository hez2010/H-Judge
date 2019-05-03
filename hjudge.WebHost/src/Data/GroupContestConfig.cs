namespace hjudge.WebHost.Data
{
    public partial class GroupContestConfig
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ContestId { get; set; }

#nullable disable
        public Contest Contest { get; set; }
        public Group Group { get; set; }
#nullable enable
    }
}
