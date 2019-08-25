using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hjudge.JudgeHost
{
    class JudgeQueue
    {
        private static readonly ConcurrentPriorityQueue<JudgeInfo> pools = new ConcurrentPriorityQueue<JudgeInfo>();
        private static readonly ConcurrentDictionary<(string DataCacheDir, string FileName), DateTime> fileCache = new ConcurrentDictionary<(string DataCacheDir, string FileName), DateTime>();
        public static SemaphoreSlim? Semaphore { get; set; }
        public static Task QueueJudgeAsync(JudgeInfo info)
        {
            if (Semaphore == null) throw new InvalidOperationException("JudgeQueue.Semaphore cannot be null.");
            Console.WriteLine($"{DateTime.Now}: Received judge request");
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
            if (Program.JudgeMessageQueueFactory == null)
            {
                throw new InvalidOperationException("Program.JudgeMessageQueueFactory cannot be null.");
            }
            var (channel, options) = Program.JudgeMessageQueueFactory.GetProducer("JudgeReport");
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            channel.BasicPublish(options.Exchange, options.RoutingKey, false, props, result.SerializeJson(false));
            return Task.CompletedTask;
        }

        public static async Task JudgeQueueExecuter(string dataCacheDir, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(dataCacheDir)) Directory.CreateDirectory(dataCacheDir);
            if (Semaphore == null) throw new InvalidOperationException("JudgeQueue.Semaphore cannot be null.");
            while (!cancellationToken.IsCancellationRequested)
            {
                await Semaphore.WaitAsync();
                if (pools.TryDequeue(out var judgeInfo))
                {
                    Console.WriteLine($"{DateTime.Now}: Started judge #{judgeInfo.JudgeId}");
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
                    var workingDir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeInfo.JudgeOptions.GuidStr);

                    var filesRequired = (await VarsProcessor.FillinWorkingDirAndGetRequiredFiles(judgeInfo, workingDir)).Distinct();

                    Console.WriteLine($"{DateTime.Now}: Started downloading files for #{judgeInfo.JudgeId}");
                    var fileService = new Files.FilesClient(Program.FileHostChannel);

                    var request = new DownloadRequest();
                    var now = DateTime.Now;

                    var timeoutThreshold = TimeSpan.FromMinutes(1);
                    foreach (var i in filesRequired)
                    {
                        var cache = fileCache.Where(j => j.Key == (dataCacheDir, i));
                        if (!cache.Any())
                        {
                            request.FileNames.Add(i);
                            fileCache[(dataCacheDir, i)] = now;
                        }
                        else if (now - cache.FirstOrDefault().Value > timeoutThreshold)
                        {
                            request.FileNames.Add(i);
                            fileCache[(dataCacheDir, i)] = now;
                        }
                    }

                    var filesResponse = fileService.DownloadFiles(request);

                    while (await filesResponse.ResponseStream.MoveNext(default))
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

                    Console.WriteLine($"{DateTime.Now}: Finished downloading files for #{judgeInfo.JudgeId}");

                    var result = new JudgeResult { JudgePoints = null };
                    try
                    {
                        result = await judge.JudgeAsync(judgeInfo.BuildOptions, judgeInfo.JudgeOptions, workingDir, dataCacheDir);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now}: {ex.Message}");
                        Console.WriteLine("-------------------");
                        Console.WriteLine(ex.StackTrace);
                    }
                    Console.WriteLine($"Finished judge #{judgeInfo.JudgeId}");
                    await ReportJudgeResultAsync(new JudgeReportInfo
                    {
                        JudgeId = judgeInfo.JudgeId,
                        JudgeResult = result,
                        Type = JudgeReportInfo.ReportType.PostJudge
                    });
                    Console.WriteLine($"Reported judge #{judgeInfo.JudgeId}");
                    if (cancellationToken.IsCancellationRequested) 
                    {
                        Directory.Delete(dataCacheDir, true);
                        break;
                    }
                }
            }
        }
    }
}
