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
        private static readonly ConcurrentDictionary<(string DataCacheDir, string FileName), long> fileCache = new ConcurrentDictionary<(string DataCacheDir, string FileName), long>();
        public static SemaphoreSlim? Semaphore { get; set; }
        public static Task QueueJudgeAsync(JudgeInfo info)
        {
            if (Semaphore == null) throw new InvalidOperationException("JudgeQueue.Semaphore cannot be null.");
            Console.WriteLine($"{DateTime.Now}: Received judge request #{info.JudgeId}");
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

        public static async Task JudgeQueueExecutor(string dataCacheDir, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(dataCacheDir)) Directory.CreateDirectory(dataCacheDir);
            if (Semaphore == null) throw new InvalidOperationException("JudgeQueue.Semaphore cannot be null.");
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Semaphore.WaitAsync(cancellationToken);
                    while (pools.TryDequeue(out var judgeInfo))
                    {
                        Console.WriteLine($"{DateTime.Now}: Started judge #{judgeInfo.JudgeId}");
                        await ReportJudgeResultAsync(new JudgeReportInfo
                        {
                            JudgeId = judgeInfo.JudgeId,
                            JudgeResult = null,
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

                        var filesRequired =
                            (await VarsProcessor.FillinWorkingDirAndGetRequiredFiles(judgeInfo, workingDir)).Distinct();

                        Console.WriteLine($"{DateTime.Now}: Started downloading files for #{judgeInfo.JudgeId}");
                        var fileService = new Files.FilesClient(Program.FileHostChannel);

                        var listRequest = new ListExactRequest();
                        listRequest.FileNames.AddRange(filesRequired);

                        var fileInfos =
                            (await fileService.ListExactFilesAsync(listRequest))
                            .FileInfos.ToDictionary(i => i.FileName, i => i.LastModified);

                        var request = new DownloadRequest();

                        foreach (var i in filesRequired)
                        {
                            var cache = fileCache.Where(j => j.Key == (dataCacheDir, i)).ToList();
                            if (!fileInfos.ContainsKey(i)) continue;
                            if (!cache.Any())
                            {
                                request.FileNames.Add(i);
                                fileCache[(dataCacheDir, i)] = fileInfos[i];
                            }
                            else if (cache.FirstOrDefault().Value != fileInfos[i])
                            {
                                request.FileNames.Add(i);
                                fileCache[(dataCacheDir, i)] = fileInfos[i];
                            }
                        }

                        var filesResponse = fileService.DownloadFiles(request, null, null, cancellationToken);

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
                                    await using var fs = new FileStream(fileName, mode, FileAccess.ReadWrite,
                                        FileShare.None);
                                    i.Content.WriteTo(fs);
                                    await fs.FlushAsync(cancellationToken);
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
                            result = await judge.JudgeAsync(judgeInfo.BuildOptions, judgeInfo.JudgeOptions, workingDir,
                                dataCacheDir);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now}: Judge Error! {ex.Message}");
                            Console.WriteLine("-------------------");
                            Console.WriteLine(ex.StackTrace);
                        }

                        Console.WriteLine($"{DateTime.Now}: Finished judge #{judgeInfo.JudgeId}");
                        await ReportJudgeResultAsync(new JudgeReportInfo
                        {
                            JudgeId = judgeInfo.JudgeId,
                            JudgeResult = result,
                            Type = JudgeReportInfo.ReportType.PostJudge
                        });
                        Console.WriteLine($"{DateTime.Now}: Reported judge #{judgeInfo.JudgeId}");
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Directory.Delete(dataCacheDir, true);
                            break;
                        }
                    }
                }
            }
            catch
            {
                while (pools.TryDequeue(out var info))
                {
                    await ReportJudgeResultAsync(new JudgeReportInfo
                    {
                        JudgeId = info.JudgeId,
                        JudgeResult = null,
                        Type = JudgeReportInfo.ReportType.PostJudge
                    });
                }
            }
        }
    }
}
