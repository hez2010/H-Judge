using hjudge.Core;
using hjudge.Shared.Utils;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjudge.WebHost.Utils
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
            return re.Matches(input).Cast<object>().Aggregate(string.Empty, (current, t) => current + t) ?? string.Empty;
        }

        private static string? ReplaceFileName(string? input, List<string> fileNames, string extName)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var str = input;
            for (var j = 0; j < fileNames.Count; j++)
            {
                str = str.Replace($"${{file:{j}}}", fileNames[j]);
            }
            return str.Replace("${file}", fileNames[0]).Replace("${extension}", extName);
        }

        public static async Task<(JudgeOptionsBuilder JudgeOptionsBuilder, BuildOptionsBuilder BuildOptionsBuilder)> GetOptionBuilders(IProblemService problemService, Judge judge, List<LanguageConfig> languageConfig)
        {
            var problem = await problemService.GetProblemAsync(judge.ProblemId);
            if (problem == null) throw new InvalidOperationException("Problem doesn't exists.");
            var problemConfig = problem.Config.DeserializeJson<ProblemConfig>(false);
            var buildOptionsBuilder = new BuildOptionsBuilder();

            var sources = judge.Content.DeserializeJson<List<Source>>();

            var ext = languageConfig
                .FirstOrDefault(i => i.Name == judge.Language)
                ?.Extensions?.Split(',', StringSplitOptions.RemoveEmptyEntries)[0]?.Trim() ?? string.Empty;

            var datadir = $"R:Data/{problem.Id}"; // R: remote
            var name = AlphaNumberFilter(problem.Name);

            foreach (var i in sources)
                buildOptionsBuilder.AddSource(i.Content, i.FileName
                            .Replace("${name}", name)
                            .Replace("${extension}", ext));

            var fileNames = sources.Select(i => i.FileName).ToList();

            if (problem.Type == 1)
            {
                var judgeOptionsBuilder = new CodeJudgeOptionsBuilder();

                var outputfile = $"${{workingdir}}/{judgeOptionsBuilder.GuidStr}.exe";

                judgeOptionsBuilder.UseComparingOptions(options =>
                {
                    options.IgnoreLineTailWhiteSpaces = problemConfig.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    options.IgnoreTextTailLineFeeds = problemConfig.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                if (problemConfig.ExtraFiles.Count != 0)
                {
                    judgeOptionsBuilder.UseExtraFiles(problemConfig.ExtraFiles.Select(
                        i => ReplaceFileName(i.Replace("${datadir}", datadir)
                             .Replace("${outputfile}", outputfile)
                             .Replace("${name}", name), fileNames, ext) ?? string.Empty)
                        .ToList());
                }

                if (!string.IsNullOrWhiteSpace(problemConfig.SpecialJudge))
                {
                    judgeOptionsBuilder.UseSpecialJudge(options =>
                    {
                        options.Exec = ReplaceFileName(problemConfig.SpecialJudge
                            .Replace("${datadir}", datadir)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                        options.UseWorkingDir = true;
                        options.UseOutputFile = true;
                        options.UseStdInputFile = true;
                        options.UseStdOutputFile = true;
                    });
                }

                for (var i = 0; i < problemConfig.Points.Count; i++)
                {
                    var point = problemConfig.Points[i];
                    judgeOptionsBuilder.AddDataPoint(new DataPoint
                    {
                        MemoryLimit = point.MemoryLimit,
                        Score = point.Score,
                        TimeLimit = point.TimeLimit,
                        StdInFile = ReplaceFileName(point.StdInFile
                                ?.Replace("${datadir}", datadir)
                                .Replace("${outputfile}", outputfile)
                                .Replace("${name}", name)
                                .Replace("${index0}", i.ToString())
                                .Replace("${index}", (i + 1).ToString()), fileNames, ext) ?? string.Empty,
                        StdOutFile = ReplaceFileName(point.StdOutFile
                                ?.Replace("${datadir}", datadir)
                                .Replace("${outputfile}", outputfile)
                                .Replace("${name}", name)
                                .Replace("${index0}", i.ToString())
                                .Replace("${index}", (i + 1).ToString()), fileNames, ext) ?? string.Empty
                    });
                }
                judgeOptionsBuilder.UseInputFileName((string.IsNullOrWhiteSpace(problemConfig.InputFileName) ? null : problemConfig.InputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionsBuilder.GuidStr}.in");
                judgeOptionsBuilder.UseOutputFileName((string.IsNullOrWhiteSpace(problemConfig.OutputFileName) ? null : problemConfig.OutputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionsBuilder.GuidStr}.out");
                if (problemConfig.UseStdIO)
                {
                    judgeOptionsBuilder.UseStdIO();
                }

                if (languageConfig.Any(i => i.Name == judge.Language))
                {
                    var lang = languageConfig.FirstOrDefault(i => i.Name == judge.Language);
                    var args = problemConfig.CompileArgs?.Split('\n')?.FirstOrDefault(i => i.StartsWith($"[{judge.Language}]"));
                    if (args != null)
                    {
                        args = args.Substring((judge.Language?.Length ?? 0) + 2);
                    }
                    if (!string.IsNullOrWhiteSpace(lang?.RunExec))
                    {
                        judgeOptionsBuilder.UseRunOptions(options =>
                        {
                            options.Exec = ReplaceFileName((string.IsNullOrWhiteSpace(lang.RunExec) ? null : lang.RunExec)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                            options.Args = ReplaceFileName((string.IsNullOrWhiteSpace(lang.RunArgs) ? null : lang.RunArgs)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(lang?.CompilerExec))
                    {
                        buildOptionsBuilder.UseCompiler(options =>
                        {
                            options.Args = ReplaceFileName((string.IsNullOrWhiteSpace(args) ? null : args)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext)
                                ??
                                ReplaceFileName((string.IsNullOrWhiteSpace(lang.CompilerArgs) ? null : lang.CompilerArgs)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                            options.Exec = ReplaceFileName((string.IsNullOrWhiteSpace(lang.CompilerExec) ? null : lang.CompilerExec)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                            options.OutputFile = outputfile;

                            if (!string.IsNullOrWhiteSpace(lang.CompilerProblemMatcher))
                            {
                                options.ProblemMatcher = new ProblemMatcher
                                {
                                    MatchPatterns = lang.CompilerProblemMatcher,
                                    DisplayFormat = lang.CompilerDisplayFormat
                                };
                            }
                            options.ReadStdOutput = lang.CompilerReadStdOutput;
                            options.ReadStdError = lang.CompilerReadStdError;
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(lang?.StaticCheckExec))
                    {
                        buildOptionsBuilder.UseStaticCheck(options =>
                        {
                            options.Args = ReplaceFileName((string.IsNullOrWhiteSpace(lang.StaticCheckArgs) ? null : lang.StaticCheckArgs)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;

                            options.Exec = ReplaceFileName((string.IsNullOrWhiteSpace(lang.StaticCheckExec) ? null : lang.StaticCheckExec)
                                    ?.Replace("${datadir}", datadir)
                                    .Replace("${outputfile}", outputfile)
                                    .Replace("${name}", name), fileNames, ext) ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(lang.StaticCheckProblemMatcher))
                            {
                                options.ProblemMatcher = new ProblemMatcher
                                {
                                    MatchPatterns = lang.StaticCheckProblemMatcher,
                                    DisplayFormat = lang.StaticCheckDisplayFormat
                                };
                            }
                            options.ReadStdOutput = lang.StaticCheckReadStdOutput;
                            options.ReadStdError = lang.StaticCheckReadStdError;
                        });
                    }

                    if (lang != null)
                    {
                        judgeOptionsBuilder.UseStdErrBehavior(lang.StandardErrorBehavior);
                        judgeOptionsBuilder.UseActiveProcessLimit(lang.ActiveProcessLimit);
                    }
                }
                return (judgeOptionsBuilder, buildOptionsBuilder);
            }
            else
            {
                var judgeOptionsBuilder = new AnswerJudgeOptionsBuilder();

                judgeOptionsBuilder.UseComparingOptions(options =>
                {
                    options.IgnoreLineTailWhiteSpaces = problemConfig.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    options.IgnoreTextTailLineFeeds = problemConfig.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                judgeOptionsBuilder.UseAnswerPoint(new AnswerPoint
                {
                    AnswerFile = ReplaceFileName((string.IsNullOrWhiteSpace(problemConfig.Answer.AnswerFile) ? null : problemConfig.Answer.AnswerFile)
                            ?.Replace("${datadir}", datadir)
                            .Replace("${outputfile}", "${file}")
                            .Replace("${name}", name)
                            .Replace("${index0}", "0")
                            .Replace("${index}", "1"), fileNames, ext)?? string.Empty,
                    Score = problemConfig.Answer.Score
                });

                if (problemConfig.ExtraFiles.Count != 0)
                {
                    judgeOptionsBuilder.UseExtraFiles(problemConfig.ExtraFiles.Select(
                        i => ReplaceFileName(i
                            .Replace("${datadir}", datadir)
                            .Replace("${outputfile}", "${file}")
                            .Replace("${name}", name), fileNames, ext) ?? string.Empty)
                            .ToList());
                }

                if (!string.IsNullOrWhiteSpace(problemConfig.SpecialJudge))
                {
                    judgeOptionsBuilder.UseSpecialJudge(options =>
                    {
                        options.Exec = ReplaceFileName(problemConfig.SpecialJudge
                            .Replace("${datadir}", datadir)
                            .Replace("${outputfile}", "${file}")
                            .Replace("${name}", name), fileNames, ext) ?? string.Empty;
                        options.UseWorkingDir = true;
                        options.UseOutputFile = true;
                        options.UseStdInputFile = true;
                        options.UseStdOutputFile = true;
                    });
                }
                return (judgeOptionsBuilder, buildOptionsBuilder);
            }
        }
    }
}
