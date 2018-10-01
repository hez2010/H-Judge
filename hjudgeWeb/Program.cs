using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeWeb
{
    public class Program
    {
        private static readonly DbContextOptionsBuilder<ApplicationDbContext> dbContextOptionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        private static readonly SystemConfiguration systemConfiguration = JsonConvert.DeserializeObject<SystemConfiguration>(File.ReadAllText("AppData/Config.json") ?? string.Empty);

        public static async Task Main(string[] args)
        {
            Languages.LanguageConfigurations.Add(new LanguageConfiguration
            {
                Name = "C++"
            });
            Languages.LanguageConfigurations.Add(new LanguageConfiguration
            {
                Name = "C"
            });
            var connectionString = JsonConvert
                .DeserializeAnonymousType(File.ReadAllText("appsettings.json"),
                    new { ConnectionStrings = new { DefaultConnection = "" } }).ConnectionStrings.DefaultConnection;
            dbContextOptionsBuilder.UseSqlite(connectionString);

            using (var db = new ApplicationDbContext(dbContextOptionsBuilder.Options))
            {
                foreach (var i in db.Judge.Where(i => i.ResultType == (int)ResultCode.Judging))
                {
                    i.ResultType = (int)ResultCode.Unknown_Error;
                }

                await db.SaveChangesAsync();
                foreach (var i in db.Judge.Where(i => i.ResultType == (int)ResultCode.Pending).Select(i => i.Id))
                {
                    JudgeQueue.Enqueue(i);
                }
            }

            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                new Thread(async () => await JudgeThread()).Start();
            }

            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }





        public static readonly ConcurrentQueue<int> JudgeQueue = new ConcurrentQueue<int>();

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
                var datadir = Path.Combine(Environment.CurrentDirectory, "Data", problem.Id.ToString());
                var workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOptionBuilder.GuidStr);
                var file = Path.Combine(workingdir, $"{judgeOptionBuilder.GuidStr}{Languages.LanguageConfigurations.FirstOrDefault(i => i.Name == judge.Language)?.Extensions[0]}");
                var outputfile = Path.Combine(workingdir, judgeOptionBuilder.GuidStr + ".exe");
                var name = AlphaNumberFilter(problem.Name);

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

                if (!string.IsNullOrEmpty(config.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = config.SpecialJudge
                            .Replace("${datadir}", datadir)
                            .Replace("${workingdir}", workingdir)
                            .Replace("${file}", file)
                            .Replace("${outputfile}", outputfile)
                            .Replace("${name}", name);
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
                judgeOptionBuilder.SetInputFileName(config.InputFileName.Replace("${name}", name));
                judgeOptionBuilder.SetOutputFileName(config.OutputFileName.Replace("${name}", name));
                if (config.UseStdIO)
                {
                    judgeOptionBuilder.UseStdIO();
                }

                if (systemConfiguration.Compilers.ContainsKey(judge.Language))
                {
                    var args = config.CompileArgs?.Split('\n')?.FirstOrDefault(i => i.StartsWith($"[{judge.Language}]"));

                    judgeOptionBuilder.SetRunOption(option =>
                    {
                        var runConfig = systemConfiguration.Execs[judge.Language];
                        option.Exec = runConfig.Executable
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;
                        option.Args = runConfig.Arguments
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;
                    });

                    buildOptionBuilder.UseCompiler(option =>
                    {
                        var compilerConfig = systemConfiguration.Compilers[judge.Language];
                        option.Args = args
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name)
                            ??
                            compilerConfig.Arguments
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;
                        option.Exec = compilerConfig.Executable
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;
                        option.OutputFile = outputfile;

                        if (!string.IsNullOrEmpty(compilerConfig.ProblemMatcher))
                        {
                            option.ProblemMatcher = new ProblemMatcher
                            {
                                MatchPatterns = compilerConfig.ProblemMatcher,
                                DisplayFormat = compilerConfig.DisplayFormat
                            };
                        }
                        option.ReadStdOutput = compilerConfig.ReadStdOutput;
                        option.ReadStdError = compilerConfig.ReadStdError;
                    });
                }

                if (systemConfiguration.StaticChecks.ContainsKey(judge.Language))
                {
                    buildOptionBuilder.UseStaticCheck(option =>
                    {
                        var staticCheckConfig = systemConfiguration.StaticChecks[judge.Language];
                        option.Args = staticCheckConfig.Arguments
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;

                        option.Exec = staticCheckConfig.Executable
                                ?.Replace("${datadir}", datadir)
                                ?.Replace("${workingdir}", workingdir)
                                ?.Replace("${file}", file)
                                ?.Replace("${outputfile}", outputfile)
                                ?.Replace("${name}", name) ?? string.Empty;

                        if (!string.IsNullOrEmpty(staticCheckConfig.ProblemMatcher))
                        {
                            option.ProblemMatcher = new ProblemMatcher
                            {
                                MatchPatterns = staticCheckConfig.ProblemMatcher,
                                DisplayFormat = staticCheckConfig.DisplayFormat
                            };
                        }
                        option.ReadStdError = staticCheckConfig.ReadStdError;
                        option.ReadStdOutput = staticCheckConfig.ReadStdOutput;
                    });
                }
                return (judgeOptionBuilder, buildOptionBuilder);
            }
            else
            {
                var judgeOptionBuilder = new AnswerJudgeOptionBuilder();
                var datadir = Path.Combine(Environment.CurrentDirectory, "Data", problem.Id.ToString());
                var workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOptionBuilder.GuidStr);
                var file = Path.Combine(workingdir, $"{judgeOptionBuilder.GuidStr}{Languages.LanguageConfigurations.FirstOrDefault(i => i.Name == judge.Language)?.Extensions[0]}");
                var outputfile = Path.Combine(workingdir, judgeOptionBuilder.GuidStr + ".exe");
                var name = AlphaNumberFilter(problem.Name);

                judgeOptionBuilder.UseComparingOption(option =>
                {
                    option.IgnoreLineTailWhiteSpaces = config.ComparingOptions.IgnoreLineTailWhiteSpaces;
                    option.IgnoreTextTailLineFeeds = config.ComparingOptions.IgnoreTextTailLineFeeds;
                });

                judgeOptionBuilder.UseAnswerPoint(new AnswerPoint
                {
                    AnswerFile = config.Answer.AnswerFile
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

                if (!string.IsNullOrEmpty(config.SpecialJudge))
                {
                    judgeOptionBuilder.UseSpecialJudge(option =>
                    {
                        option.Exec = config.SpecialJudge
                            ?.Replace("${datadir}", datadir)
                            ?.Replace("${workingdir}", workingdir)
                            ?.Replace("${file}", file)
                            ?.Replace("${outputfile}", outputfile)
                            ?.Replace("${name}", name) ?? string.Empty;
                        option.UseOutputFile = true;
                        option.UseStdInputFile = true;
                        option.UseStdOutputFile = true;
                    });
                }
                return (judgeOptionBuilder, buildOptionBuilder);
            }
        }

        private static async Task JudgeThread()
        {
            ApplicationDbContext db = null;
            while (Environment.HasShutdownStarted)
            {
                while (JudgeQueue.TryDequeue(out int id))
                {
                    if (db == null)
                    {
                        db = new ApplicationDbContext(dbContextOptionsBuilder.Options);
                    }

                    var judge = await db.Judge.FindAsync(id);
                    if (judge == null)
                    {
                        continue;
                    }

                    judge.ResultType = (int)ResultCode.Judging;
                    await db.SaveChangesAsync();

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

                    var (judgeOptionBuilder, buildOptionBuilder) = GetOptionBuilders(problem, judge, config);

                    var judgeMain = new JudgeMain(systemConfiguration.Environments);
                    var result = await judgeMain.JudgeAsync(buildOptionBuilder.Build(), judgeOptionBuilder.Build());
                    judge.Result = JsonConvert.SerializeObject(result);
                    judge.ResultType = (int)new Func<ResultCode>(() =>
                    {
                        if (result.JudgePoints == null)
                        {
                            return ResultCode.Judging;
                        }

                        if (result.JudgePoints.Count == 0 || result.JudgePoints.All(i => i.Result == ResultCode.Accepted))
                        {
                            return ResultCode.Accepted;
                        }

                        var mostPresentTimes =
                            result.JudgePoints.Select(i => i.Result).Distinct().Max(i =>
                                result.JudgePoints.Count(j => j.Result == i && j.Result != ResultCode.Accepted));
                        var mostPresent =
                            result.JudgePoints.Select(i => i.Result).Distinct().FirstOrDefault(
                                i => result.JudgePoints.Count(j => j.Result == i && j.Result != ResultCode.Accepted) ==
                                     mostPresentTimes
                            );
                        return mostPresent;
                    }).Invoke();
                    judge.FullScore = result.JudgePoints?.Sum(i => i.Score) ?? 0;
                    await db.SaveChangesAsync();
                }
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
                await Task.Delay(1000);
            }
        }

    }
}
