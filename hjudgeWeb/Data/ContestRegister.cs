﻿namespace hjudgeWeb.Data
{
    public partial class ContestRegister
    {
        public int Id { get; set; }
        public int? ContestId { get; set; }
        public string UserId { get; set; }

        public Contest Contest { get; set; }
    }
}
