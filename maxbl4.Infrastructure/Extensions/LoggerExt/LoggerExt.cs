using System;
using System.Threading.Tasks;
using Serilog;

namespace maxbl4.Infrastructure.Extensions.LoggerExt
{
    public static class LoggerExt
    {
        public static void Swallow(this ILogger logger, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Warning swallowed");
            }
        }
        
        public static Task Swallow(this ILogger logger, Func<Task> func)
        {
            try
            {
                return func().ContinueWith(x => logger.Warning(x.Exception, "Warning swallowed"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Warning swallowed");
                return Task.CompletedTask;
            }
        }
        
        public static Task SwallowError(this ILogger logger, Func<Task> func)
        {
            try
            {
                return func().ContinueWith(x => logger.Error(x.Exception, "Error swallowed"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error swallowed");
                return Task.CompletedTask;
            }
        }
        
        public static void SwallowError(this ILogger logger, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error swallowed");
            }
        }
    }
}