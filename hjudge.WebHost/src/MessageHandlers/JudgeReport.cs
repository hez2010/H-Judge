using hjudge.Shared.Judge;
using hjudge.WebHost.Services;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using hjudge.Shared.Utils;
using System;
using System.Threading.Tasks;
using hjudge.WebHost.Extensions;

namespace hjudge.WebHost.MessageHandlers
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

                judgeService = MessageHandlersServiceExtensions.ServiceProvider.GetService<IJudgeService>();
                if (judgeService == null) throw new InvalidOperationException("IJudgeService was not registed.");

                await judgeService.UpdateJudgeResultAsync(info.JudgeId, info.Type, info.JudgeResult);
                consumer.Model.BasicAck(args.DeliveryTag, false);
            }
        }
    }
}
