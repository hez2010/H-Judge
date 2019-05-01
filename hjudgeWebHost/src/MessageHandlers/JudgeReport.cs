using hjudgeJudgeHost;
using hjudgeWebHost.Services;
using hjudgeWebHost.Utils;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.MessageHandlers
{
    public class JudgeReport
    {
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

                judgeService = Startup.Provider.GetService<IJudgeService>();
                if (judgeService == null) throw new InvalidOperationException("IJudgeService was not registed.");

                consumer.Model.BasicAck(args.DeliveryTag, false);
                if (info.JudgeResult != null) await judgeService.UpdateJudgeResultAsync(info.JudgeId, info.Type, info.JudgeResult);
            }
        }
    }
}
