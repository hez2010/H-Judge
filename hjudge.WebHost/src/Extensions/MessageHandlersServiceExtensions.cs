using Microsoft.Extensions.DependencyInjection;
using System;

namespace hjudge.WebHost.Extensions
{
    public static class MessageHandlersServiceExtensions
    {
        private static IServiceCollection? serviceCollection;
        private static IServiceProvider? serviceProvider;
        public static IServiceProvider? ServiceProvider => serviceProvider ?? GetServiceProvider();
        public static IServiceCollection AddMessageHandlers(this IServiceCollection collection)
        {
            serviceCollection = collection;
            return collection;
        }
        private static IServiceProvider GetServiceProvider()
        {
            serviceProvider = serviceCollection?.BuildServiceProvider();
            if (serviceProvider == null) throw new InvalidOperationException("Please invoke service.AddMessageHandlers() in you ConfigureService method");

            return serviceProvider;
        }
    }
}
