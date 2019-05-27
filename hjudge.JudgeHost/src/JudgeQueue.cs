using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.Utils;
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

        public static async Task JudgeQueueExecuter(CancellationToken cancellationToken)
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
                    if (judgeInfo.BuildOptions == null || judgeInfo.JudgeOptions == null)
                    {
                        await ReportJudgeResultAsync(new JudgeReportInfo
                        {
                            JudgeId = judgeInfo.JudgeId,
                            JudgeResult = new JudgeResult { JudgePoints = null },
                            Type = JudgeReportInfo.ReportType.PostJudge
                        });
                        continue;
                    }

                    var varsTable = new Dictionary<string, string>
                    {
                        ["\\${workingdir:(.*?)}"] = JudgeMain.GetWorkingDir(Path.GetTempPath(), judgeInfo.JudgeOptions.GuidStr)
                    };
                    var filesRequired = (await VarsProcessor.FillinVarsAndFetchFiles(judgeInfo, varsTable)).Distinct();
                    var fileService = new Files.FilesClient(Program.FileHostChannel);
                    var request = new DownloadRequest();
                    request.Info.AddRange(filesRequired.Select(i => new DownloadInfo { FileName = i }));
                    var filesResponse = fileService.DownloadFiles(request);
                    while (await filesResponse.ResponseStream.MoveNext())
                    {
                        foreach (var i in filesResponse.ResponseStream.Current.Result)
                        {
                            //TODO: datadir
                            i.Content.WriteTo(null);
                        }
                    }

                    var result = new JudgeResult { JudgePoints = null };
                    try
                    {
                        result = await judge.JudgeAsync(judgeInfo.BuildOptions, judgeInfo.JudgeOptions, Path.GetTempPath());
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
