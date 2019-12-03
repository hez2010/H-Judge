using System;

namespace hjudge.WebHost.Models.Contest
{
    public class JoinQuitContestModel
    {
        public string[] UserIds { get; set; } = Array.Empty<string>();
        public int ContestId { get; set; }
    }
}
