using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hjudge.JudgeHost
{
    class JudgeQueue
    {
        private static readonly ConcurrentPriorityQueue<JudgeInfo> pools = new ConcurrentPriorityQueue<JudgeInfo>();
        private static readonly ConcurrentDictionary<string, DateTime> fileCache = new ConcurrentDictionary<string, DateTime>();
        public static SemaphoreSlim Semaphore { get; set; }
        public static Task QueueJudgeAsync(JudgeInfo info)
        {
            pools.Enqueue(info, info.Priority);
            try
            {
                Semaphore.Release();
            }
            catch { /* ignored */ }
            return Task.CompletedTask;
        }

        private static Task ReportJudgeResultAsync(JudgeReportInfo result)
        {
            var (channel, options) = Program.JudgeMessageQueueFactory.GetProducer("JudgeReport");
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            channel.BasicPublish(options.Exchange, options.RoutingKey, false, props, result.SerializeJson(false));
            return Task.CompletedTask;
        }


        public static async Task JudgeQueueExecuter(string dataCacheDir, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Semaphore.WaitAsync();
                if (pools.TryDequeue(out var judgeInfo))
                {
                    await ReportJudgeResultAsync(new JudgeReportInfo
                    {
                        JudgeId = judgeInfo.JudgeId,
                        Type = JudgeReportInfo.ReportType.PreJudge
                    });

                    var judge = new JudgeMain();
                    if (judgeInfo.JudgeOptions == null || judgeInfo.BuildOptions == null)
                    {
                        await ReportJudgeResultAsync(new JudgeReportInfo
                        {
                            JudgeId = judgeInfo.JudgeId,
                            JudgeResult = new JudgeResult { JudgePoints = null },
                            Type = JudgeReportInfo.ReportType.PostJudge
                        });
                        continue;
                    }
                    var workingdir = JudgeMain.GetWorkingDir(Path.GetTempPath(), judgeInfo.JudgeOptions.GuidStr);
                    var varsTable = new Dictionary<string, string>
                    {
                        ["\\${workingdir:(.*?)}"] = workingdir
                    };
                    if (judgeInfo.JudgeOptions != null)
                    {
                        for (var i = 0; i < judgeInfo.JudgeOptions.DataPoints.Count; i++)
                        {
                            judgeInfo.JudgeOptions.DataPoints[i].StdInFile = judgeInfo.JudgeOptions.DataPoints[i].StdInFile
                                .Replace("${index0}", i.ToString())
                                .Replace("${index}", (i + 1).ToString());
                            judgeInfo.JudgeOptions.DataPoints[i].StdOutFile = judgeInfo.JudgeOptions.DataPoints[i].StdOutFile
                                .Replace("${index0}", i.ToString())
                                .Replace("${index}", (i + 1).ToString());
                        }
                        if (judgeInfo.JudgeOptions.AnswerPoint != null)
                        {
                            judgeInfo.JudgeOptions.AnswerPoint.AnswerFile = judgeInfo.JudgeOptions.AnswerPoint.AnswerFile
                                .Replace("${index0}", "0")
                                .Replace("${index}", "1");
                        }
                    }
                    var filesRequired = (await VarsProcessor.FillinVarsAndFetchFiles(judgeInfo, varsTable)).Distinct();

                    var fileService = new Files.FilesClient(Program.FileHostChannel);

                    var request = new DownloadRequest();
                    var now = DateTime.Now;

                    var timeoutThreshold = TimeSpan.FromMinutes(10);
                    foreach (var i in filesRequired)
                    {
                        var cache = fileCache.Where(j => j.Key == i);
                        if (!cache.Any())
                        {
                            request.Info.Add(new DownloadInfo { FileName = i });
                            fileCache[i] = now;
                        }
                        else if (now - cache.FirstOrDefault().Value > timeoutThreshold)
                        {
                            request.Info.Add(new DownloadInfo { FileName = i });
                            fileCache[i] = now;
                        }
                    }

                    var filesResponse = fileService.DownloadFiles(request);

                    while (await filesResponse.ResponseStream.MoveNext())
                    {
                        foreach (var i in filesResponse.ResponseStream.Current.Result)
                        {
                            var fileName = Path.Combine(dataCacheDir, JudgeMain.EscapeFileName(i.FileName));
                            FileMode mode;
                            if (File.Exists(fileName)) mode = FileMode.Truncate;
                            else mode = FileMode.CreateNew;
                            try
                            {
                                using var fs = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.None);
                                i.Content.WriteTo(fs);
                                await fs.FlushAsync();
                                fs.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }

                    var result = new JudgeResult { JudgePoints = null };
                    try
                    {
                        //TODO: remove !
                        result = await judge.JudgeAsync(judgeInfo.BuildOptions, judgeInfo.JudgeOptions!, Path.GetTempPath(), dataCacheDir);
                    }
                    catch
                    {
                        // ignored
                    }
                    await ReportJudgeResultAsync(new JudgeReportInfo
                    {
                        JudgeId = judgeInfo.JudgeId,
                        JudgeResult = result,
                        Type = JudgeReportInfo.ReportType.PostJudge
                    });
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }
        }
    }
}
