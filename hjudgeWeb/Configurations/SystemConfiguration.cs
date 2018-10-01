using System;
using System.Collections.Generic;

namespace hjudgeWeb.Configurations
{
    public class ExecutableFile : ICloneable
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class Compiler : ICloneable
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public string ProblemMatcher { get; set; }
        public string DisplayFormat { get; set; }
        public bool ReadStdOutput { get; set; }
        public bool ReadStdError { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class StaticCheck : ICloneable
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public string ProblemMatcher { get; set; }
        public string DisplayFormat { get; set; }
        public bool ReadStdOutput { get; set; }
        public bool ReadStdError { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class SystemConfiguration
    {
        public string Environments;

        public SystemConfiguration()
        {
            StaticChecks = new Dictionary<string, StaticCheck>();
            Compilers = new Dictionary<string, Compiler>();
            Execs = new Dictionary<string, ExecutableFile>();
            Extensions = new Dictionary<string, string[]>();
        }

        /// <summary>
        ///     language -> extensions
        /// </summary>
        public Dictionary<string, string[]> Extensions { get; set; }

        public Dictionary<string, ExecutableFile> Execs { get; set; }
        public Dictionary<string, Compiler> Compilers { get; set; }
        public Dictionary<string, StaticCheck> StaticChecks { get; set; }
    }
}
