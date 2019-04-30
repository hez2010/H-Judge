using hjudgeCore;
using hjudgeWebHost.Services;
using hjudgeWebHost.Utils;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.MessageHandlers
{
    public class JudgeReport
    {
        public class JudgeReportInfo
        {
            public int JudgeId { get; set; }
            public JudgeResult? JudgeResult { get; set; }
        }

        private static IJudgeService? judgeService;

        public static async Task JudgeReport_Received(object sender, BasicDeliverEventArgs args)
        {
            if (sender is AsyncEventingBasicConsumer consumer)
            {
                JudgeReportInfo info;

                try
                {
                    info = args.Body.DeserializeJson<JudgeReportInfo>(false);
                }
                catch
                {
                    consumer.Model.BasicAck(args.DeliveryTag, false);
                    return;
                }

                if (judgeService == null) judgeService = Startup.Provider.GetService(typeof(IJudgeService)) as IJudgeService;
                if (judgeService == null) throw new InvalidOperationException("IJudgeService not found.");

                consumer.Model.BasicAck(args.DeliveryTag, false);
                if (info.JudgeResult != null) await judgeService.UpdateJudgeResultAsync(info.JudgeId, info.JudgeResult);
            }
        }
    }
}
