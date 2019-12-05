using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Infrastructure.Extensions.ServiceCollectionExt
{
    public static class ServiceCollectionExt
    {
        public static void RegisterHostedService<T>(this IServiceCollection serviceCollection) where T : class, IDisposable
        {
            serviceCollection.AddSingleton<T>();
            serviceCollection.AddSingleton<IHostedService, ServiceHost<T>>();
        }
        
        public static void RegisterHostedService<T,TImpl>(this IServiceCollection serviceCollection) where T : class, IDisposable where TImpl : class, T
        {
            serviceCollection.AddSingleton<T, TImpl>();
            serviceCollection.AddSingleton<IHostedService, ServiceHost<T>>();
        }
    }
}