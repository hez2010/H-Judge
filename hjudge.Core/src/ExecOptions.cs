namespace hjudge.Core
{
    class ExecOptions
    {
        public string? Exec { get; set; }
        public string? Args { get; set; }
        public string? WorkingDir { get; set; }
        public string? StdErrRedirectFile { get; set; }
        public string? InputFile { get; set; }
        public string? OutputFile { get; set; }
        public long TimeLimit { get; set; }
        public long MemoryLimit { get; set; }
        public bool IsStdIO { get; set; }
        public int ActiveProcessLimit { get; set; }
    }
}
