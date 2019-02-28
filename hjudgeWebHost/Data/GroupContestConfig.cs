namespace hjudgeWebHost.Data
{
    public partial class GroupContestConfig
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ContestId { get; set; }

        public Contest? Contest { get; set; }
        public Group? Group { get; set; }
    }
}
