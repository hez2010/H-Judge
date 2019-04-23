using hjudgeCore;
using hjudgeWebHost.Configurations;
using hjudgeWebHost.Data;
using hjudgeWebHost.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjudgeWebHost.Utils
{
    public class JudgeHelper
    {
        private static string AlphaNumberFilter(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            var re = new Regex("[A-Z]|[a-z]|[0-9]");
            return re.Matches(input)?.Cast<object>()?.Aggregate(string.Empty, (current, t) => current + t);
        }

        public static async Task<(JudgeOptionBuilder JudgeOptionBuilder, BuildOptionBuilder BuildOptionBuilder)> GetOptionBuilders(IProblemService problemService, Judge judge, IEnumerable<LanguageConfig> languageConfig)
        {
            var problem = await problemService.GetProblemAsync(judge.ProblemId);
            var problemConfig = problem.Config.DeserializeJson<ProblemConfig>(false);
            var buildOptionBuilder = new BuildOptionBuilder();
            if (problem.Type == 1)
            {
                var judgeOptionBuilder = new CodeJudgeOptionBuilder();
                var ext = languageConfig.FirstOrDefault(i => i.Name == judge.Language)?.Extensions?.Split(',', StringSplitOptions.RemoveEmptyEntries)[0]?.Trim();
                buildOptionBuilder.AddExtensionName(ext);

                var datadir = $"${{datadir:{problem.Id}}}";
                var workingdir = $"{{workingdir:{judgeOptionBuilder.GuidStr}}}";
                var file = $"{workingdir}/{judgeOptionBuilder.GuidStr}{ext}";
                var outputfile = $"{workingdir}/{judgeOptionBuilder.GuidStr}.exe";
                var name = AlphaNumberFilter(problem.Name);

                if (string.IsNullOrWhiteSpace(problemConfig.SubmitFileName))
                {
                    buildOptionBuilder.UseCustomSubmitFileName(judgeOptionBuilder.GuidStr);
                }
                else
                {
                    buildOptionBuilder.UseCustomSubmitFileName(problemConfig.SubmitFileName);
                    file = $"{workingdir}/{problemConfig.SubmitFileName}{ext}";
                }

                judgeOptionBuilder.UseComparingOption(option =>
                {
                    option.IgnoreLineTailWhiteSpaces = problemConfig.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    option.IgnoreTextTailLineFeeds = problemConfig.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                if (problemConfig.ExtraFiles.Count != 0)
                {
                    judgeOptionBuilder.UseExtraFiles(problemConfig.ExtraFiles.Select(
                        i => i
                            .Replace("${datadir}", datadir)
                            .Replace("${workingdir}", workingdir)
                            .Replace("${file}", file)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name))
                            .ToList());
                }

                if (!string.IsNullOrWhiteSpace(problemConfig.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = problemConfig.SpecialJudge
                            .Replace("${datadir}", datadir)
                            .Replace("${workingdir}", workingdir)
                            .Replace("${file}", file)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name);
                        option.UseSourceFile = true;
                        option.UseOutputFile = true;
                        option.UseStdInputFile = true;
                        option.UseStdOutputFile = true;
                    });
                }

                foreach (var point in problemConfig.Points)
                {
                    judgeOptionBuilder.AddDataPoint(new DataPoint
                    {
                        MemoryLimit = point.MemoryLimit,
                        Score = point.Score,
                        TimeLimit = point.TimeLimit,
                        StdInFile = point.StdInFile
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty,
                        StdOutFile = point.StdOutFile
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty
                    });
                }
                judgeOptionBuilder.SetInputFileName((string.IsNullOrWhiteSpace(problemConfig.InputFileName) ? null : problemConfig.InputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionBuilder.GuidStr}.in");
                judgeOptionBuilder.SetOutputFileName((string.IsNullOrWhiteSpace(problemConfig.OutputFileName) ? null : problemConfig.OutputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionBuilder.GuidStr}.out");
                if (problemConfig.UseStdIO)
                {
                    judgeOptionBuilder.UseStdIO();
                }

                if (languageConfig.Any(i => i.Name == judge.Language))
                {
                    var lang = languageConfig.FirstOrDefault(i => i.Name == judge.Language);
                    var args = problemConfig.CompileArgs?.Split('\n')?.FirstOrDefault(i => i.StartsWith($"[{judge.Language}]"));
                    if (args != null)
                    {
                        args = args.Substring(judge.Language.Length + 2);
                    }
                    if (!string.IsNullOrWhiteSpace(lang.RunExec))
                    {
                        judgeOptionBuilder.SetRunOption(option =>
                        {
                            option.Exec = (string.IsNullOrWhiteSpace(lang.RunExec) ? null : lang.RunExec)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;
                            option.Args = (string.IsNullOrWhiteSpace(lang.RunArgs) ? null : lang.RunArgs)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(lang.CompilerExec))
                    {
                        buildOptionBuilder.UseCompiler(option =>
                        {
                            option.Args = (string.IsNullOrWhiteSpace(args) ? null : args)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name)
                                ??
                                (string.IsNullOrWhiteSpace(lang.CompilerArgs) ? null : lang.CompilerArgs)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;
                            option.Exec = (string.IsNullOrWhiteSpace(lang.CompilerExec) ? null : lang.CompilerExec)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;
                            option.OutputFile = outputfile;

                            if (!string.IsNullOrWhiteSpace(lang.CompilerProblemMatcher))
                            {
                                option.ProblemMatcher = new ProblemMatcher
                                {
                                    MatchPatterns = lang.CompilerProblemMatcher,
                                    DisplayFormat = lang.CompilerDisplayFormat
                                };
                            }
                            option.ReadStdOutput = lang.CompilerReadStdOutput;
                            option.ReadStdError = lang.CompilerReadStdError;
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(lang.StaticCheckExec))
                    {
                        buildOptionBuilder.UseStaticCheck(option =>
                        {
                            option.Args = (string.IsNullOrWhiteSpace(lang.StaticCheckArgs) ? null : lang.StaticCheckArgs)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;

                            option.Exec = (string.IsNullOrWhiteSpace(lang.StaticCheckExec) ? null : lang.StaticCheckExec)
                                    ?.Replace("${datadir}", datadir)
                                    ?.Replace("${workingdir}", workingdir)
                                    ?.Replace("${file}", file)
                                    ?.Replace("${outputfile}", outputfile)
                                    ?.Replace("${name}", name) ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(lang.StaticCheckProblemMatcher))
                            {
                                option.ProblemMatcher = new ProblemMatcher
                                {
                                    MatchPatterns = lang.StaticCheckProblemMatcher,
                                    DisplayFormat = lang.StaticCheckDisplayFormat
                                };
                            }
                            option.ReadStdOutput = lang.StaticCheckReadStdOutput;
                            option.ReadStdError = lang.StaticCheckReadStdError;
                        });
                    }
                    judgeOptionBuilder.SetStdErrBehavior(lang.StandardErrorBehavior);

                    judgeOptionBuilder.SetActiveProcessLimit(lang.ActiveProcessLimit);
                }
                return (judgeOptionBuilder, buildOptionBuilder);
            }
            else
            {
                var judgeOptionBuilder = new AnswerJudgeOptionBuilder();
                var datadir = $"${{datadir:{problem.Id}}}";
                var workingdir = $"{{workingdir:{judgeOptionBuilder.GuidStr}}}";
                var file = $"{workingdir}/{judgeOptionBuilder.GuidStr}";
                var name = AlphaNumberFilter(problem.Name);

                if (string.IsNullOrWhiteSpace(problemConfig.SubmitFileName))
                {
                    buildOptionBuilder.UseCustomSubmitFileName(judgeOptionBuilder.GuidStr);
                }
                else
                {
                    buildOptionBuilder.UseCustomSubmitFileName(problemConfig.SubmitFileName);
                    file = $"{workingdir}/{problemConfig.SubmitFileName}";
                }

                var outputfile = file;

                judgeOptionBuilder.UseComparingOption(option =>
                {
                    option.IgnoreLineTailWhiteSpaces = problemConfig.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    option.IgnoreTextTailLineFeeds = problemConfig.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                judgeOptionBuilder.UseAnswerPoint(new AnswerPoint
                {
                    AnswerFile = (string.IsNullOrWhiteSpace(problemConfig.Answer.AnswerFile) ? null : problemConfig.Answer.AnswerFile)
                            ?.Replace("${datadir}", datadir)
                            ?.Replace("${workingdir}", workingdir)
                            ?.Replace("${file}", file)
                            ?.Replace("${outputfile}", outputfile)
                            ?.Replace("${name}", name) ?? string.Empty,
                    Score = problemConfig.Answer.Score
                });

                if (problemConfig.ExtraFiles.Count != 0)
                {
                    judgeOptionBuilder.UseExtraFiles(problemConfig.ExtraFiles.Select(
                        i => i
                            ?.Replace("${datadir}", datadir)
                            ?.Replace("${workingdir}", workingdir)
                            ?.Replace("${file}", file)
                            ?.Replace("${outputfile}", outputfile)
                            ?.Replace("${name}", name))
                            ?.ToList());
                }

                if (!string.IsNullOrWhiteSpace(problemConfig.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = problemConfig.SpecialJudge
                            .Replace("${datadir}", datadir)
                            .Replace("${workingdir}", workingdir)
                            .Replace("${file}", file)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name);
                        option.UseSourceFile = true;
                        option.UseOutputFile = true;
                        option.UseStdInputFile = true;
                        option.UseStdOutputFile = true;
                    });
                }
                return (judgeOptionBuilder, buildOptionBuilder);
            }
        }
    }
}
