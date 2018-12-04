using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeWeb
{
    public class JudgeQueue
    {
        //Judge queue. A tuple (Judge Id, Whether being judged before)
        public static readonly ConcurrentQueue<int> JudgeIdQueue = new ConcurrentQueue<int>();
        
        private static string AlphaNumberFilter(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            var re = new Regex("[A-Z]|[a-z]|[0-9]");
            return re.Matches(input)?.Cast<object>()?.Aggregate(string.Empty, (current, t) => current + t);
        }

        private static (JudgeOptionBuilder JudgeOptionBuilder, BuildOptionBuilder BuildOptionBuilder) GetOptionBuilders(Problem problem, Judge judge, ProblemConfiguration config)
        {
            var buildOptionBuilder = new BuildOptionBuilder();
            if (problem.Type == 1)
            {
                var judgeOptionBuilder = new CodeJudgeOptionBuilder();
                var ext = Languages.LanguageConfigurations.FirstOrDefault(i => i.Name == judge.Language)?.Extensions?.Split(',', StringSplitOptions.RemoveEmptyEntries)[0]?.Trim();
                buildOptionBuilder.AddExtensionName(ext);
                var datadir = Path.Combine(Environment.CurrentDirectory, "AppData", "Data", problem.Id.ToString());
                var workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOptionBuilder.GuidStr);
                var file = Path.Combine(workingdir, $"{judgeOptionBuilder.GuidStr}{ext}");
                var outputfile = Path.Combine(workingdir, judgeOptionBuilder.GuidStr + ".exe");
                var name = AlphaNumberFilter(problem.Name);

                if (string.IsNullOrWhiteSpace(config.SubmitFileName))
                {
                    buildOptionBuilder.UseCustomSubmitFileName(judgeOptionBuilder.GuidStr);
                }
                else
                {
                    buildOptionBuilder.UseCustomSubmitFileName(config.SubmitFileName);
                    file = Path.Combine(workingdir, $"{config.SubmitFileName}{ext}");
                }

                judgeOptionBuilder.UseComparingOption(option =>
                {
                    option.IgnoreLineTailWhiteSpaces = config.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    option.IgnoreTextTailLineFeeds = config.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                if (config.ExtraFiles.Count != 0)
                {
                    judgeOptionBuilder.UseExtraFiles(config.ExtraFiles.Select(
                        i => i
                            .Replace("${datadir}", datadir)
                            .Replace("${workingdir}", workingdir)
                            .Replace("${file}", file)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name))
                            .ToList());
                }

                if (!string.IsNullOrWhiteSpace(config.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = config.SpecialJudge
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

                foreach (var point in config.Points)
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
                judgeOptionBuilder.SetInputFileName((string.IsNullOrWhiteSpace(config.InputFileName) ? null : config.InputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionBuilder.GuidStr}.in");
                judgeOptionBuilder.SetOutputFileName((string.IsNullOrWhiteSpace(config.OutputFileName) ? null : config.OutputFileName)?.Replace("${name}", name) ?? $"test_{judgeOptionBuilder.GuidStr}.out");
                if (config.UseStdIO)
                {
                    judgeOptionBuilder.UseStdIO();
                }

                if (Languages.LanguagesList.Contains(judge.Language))
                {
                    var lang = Languages.LanguageConfigurations.FirstOrDefault(i => i.Name == judge.Language);
                    var args = config.CompileArgs?.Split('\n')?.FirstOrDefault(i => i.StartsWith($"[{judge.Language}]"));
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
                }
                return (judgeOptionBuilder, buildOptionBuilder);
            }
            else
            {
                var judgeOptionBuilder = new AnswerJudgeOptionBuilder();
                var datadir = Path.Combine(Environment.CurrentDirectory, "AppData", "Data", problem.Id.ToString());
                var workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOptionBuilder.GuidStr);
                var file = Path.Combine(workingdir, judgeOptionBuilder.GuidStr);
                var name = AlphaNumberFilter(problem.Name);

                if (string.IsNullOrWhiteSpace(config.SubmitFileName))
                {
                    buildOptionBuilder.UseCustomSubmitFileName(judgeOptionBuilder.GuidStr);
                }
                else
                {
                    buildOptionBuilder.UseCustomSubmitFileName(config.SubmitFileName);
                    file = Path.Combine(workingdir, config.SubmitFileName);
                }

                var outputfile = file;

                judgeOptionBuilder.UseComparingOption(option =>
                {
                    option.IgnoreLineTailWhiteSpaces = config.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    option.IgnoreTextTailLineFeeds = config.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                judgeOptionBuilder.UseAnswerPoint(new AnswerPoint
                {
                    AnswerFile = (string.IsNullOrWhiteSpace(config.Answer.AnswerFile) ? null : config.Answer.AnswerFile)
                            ?.Replace("${datadir}", datadir)
                            ?.Replace("${workingdir}", workingdir)
                            ?.Replace("${file}", file)
                            ?.Replace("${outputfile}", outputfile)
                            ?.Replace("${name}", name) ?? string.Empty,
                    Score = config.Answer.Score
                });

                if (config.ExtraFiles.Count != 0)
                {
                    judgeOptionBuilder.UseExtraFiles(config.ExtraFiles.Select(
                        i => i
                            ?.Replace("${datadir}", datadir)
                            ?.Replace("${workingdir}", workingdir)
                            ?.Replace("${file}", file)
                            ?.Replace("${outputfile}", outputfile)
                            ?.Replace("${name}", name))
                            ?.ToList());
                }

                if (!string.IsNullOrWhiteSpace(config.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = config.SpecialJudge
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

        public static SemaphoreSlim QueueSemaphore = new SemaphoreSlim(0, Environment.ProcessorCount);

        public static async Task JudgeThread(int threadId)
        {
            ApplicationDbContext db = null;
            var random = new Random();
            while (!Environment.HasShutdownStarted)
            {
                Console.WriteLine($"** {DateTime.Now}: Thread {threadId} start waiting semaphore");
                await QueueSemaphore.WaitAsync();
                Console.WriteLine($"** {DateTime.Now}: Thread {threadId} wait semaphore ok");
                while (JudgeIdQueue.TryDequeue(out var judgeId))
                {
                    Console.WriteLine($"** {DateTime.Now}: Judge {judgeId} started");
                    if (db == null)
                    {
                        db = new ApplicationDbContext(Program.DbContextOptionsBuilder.Options);
                    }

                    //Get judge record
                    var judge = await db.Judge.FindAsync(judgeId);
                    if (judge == null)
                    {
                        continue;
                    }

                    judge.ResultType = (int)ResultCode.Judging;
                    await db.SaveChangesAsync();

                    //Get problem information
                    var problem = await db.Problem.FindAsync(judge.ProblemId);
                    if (problem == null)
                    {
                        continue;
                    }

                    var config = JsonConvert.DeserializeObject<ProblemConfiguration>(problem.Config ?? "{}");
                    if (config == null)
                    {
                        judge.ResultType = (int)ResultCode.Problem_Config_Error;
                        await db.SaveChangesAsync();
                        continue;
                    }


                    try
                    {
                        //Build all options
                        var (judgeOptionBuilder, buildOptionBuilder) = GetOptionBuilders(problem, judge, config);
                        buildOptionBuilder.AddSource(judge.Content);
                        //Init judge task
                        var judgeMain = new JudgeMain(SystemConfiguration.Config.Environments);
                        //Judge
                        var result = await judgeMain.JudgeAsync(buildOptionBuilder.Build(), judgeOptionBuilder.Build());

                        //Save result and increase coins and experience for user
                        judge.Result = JsonConvert.SerializeObject(result);
                        judge.ResultType = (int)new Func<ResultCode>(() =>
                        {
                            if (result.JudgePoints == null)
                            {
                                return ResultCode.Judging;
                            }

                            if (result.JudgePoints.Count == 0 || result.JudgePoints.All(i => i.ResultType == ResultCode.Accepted))
                            {
                                return ResultCode.Accepted;
                            }

                            var mostPresentTimes =
                                result.JudgePoints.Select(i => i.ResultType).Distinct().Max(i =>
                                    result.JudgePoints.Count(j => j.ResultType == i && j.ResultType != ResultCode.Accepted));
                            var mostPresent =
                                result.JudgePoints.Select(i => i.ResultType).Distinct().FirstOrDefault(
                                    i => result.JudgePoints.Count(j => j.ResultType == i && j.ResultType != ResultCode.Accepted) ==
                                         mostPresentTimes
                                );
                            return mostPresent;
                        }).Invoke();
                        judge.FullScore = result.JudgePoints?.Sum(i => i.Score) ?? 0;
                        judge.JudgeCount++;
                        if (judge.JudgeCount == 1)
                        {
                            if (judge.ResultType == (int)ResultCode.Accepted)
                            {
                                if (judge.ContestId == null)
                                {
                                    problem.AcceptCount++;
                                }
                                else
                                {
                                    var problemConfig = db.ContestProblemConfig.FirstOrDefault(i => i.ContestId == judge.ContestId && i.ProblemId == problem.Id);
                                    if (problemConfig != null)
                                    {
                                        problemConfig.AcceptCount++;
                                    }
                                }
                            }
                            try
                            {
                                var user = db.Users.FirstOrDefault(i => i.Id == judge.UserId);

                                if (user != null)
                                {
                                    long dExp = 0, dCoins = 0;
                                    switch (judge.ResultType)
                                    {
                                        case (int)ResultCode.Accepted:
                                            dExp = random.Next(50, 100);
                                            dCoins = random.Next(30, 80);
                                            break;
                                        case (int)ResultCode.Presentation_Error:
                                            dExp = random.Next(30, 50);
                                            dCoins = random.Next(10, 30);
                                            break;
                                        default:
                                            dExp = random.Next(10, 30);
                                            break;
                                    }
                                    user.Coins += dCoins;
                                    user.Experience += dExp;
                                    user.AcceptedCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to update userinfo for user [{judge.UserId}]: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        judge.ResultType = (int)ResultCode.Unknown_Error;
                        judge.Result = "{}";
                        judge.AdditionalInfo = ex.Message;
                        judge.FullScore = 0;
                        Console.WriteLine("Judge Error!");
                        Console.WriteLine("----------------------------");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Source);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine(ex.TargetSite);
                        Console.WriteLine("----------------------------");
                    }
                    await db.SaveChangesAsync();
                }
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
        }

    }
}
