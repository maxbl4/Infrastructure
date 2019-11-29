﻿using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace maxbl4.Infrastructure.Extensions.LoggerExt
{
    public static class LoggerExt
    {
        private static Task SwallowImpl(this ILogger logger, Func<Task> func, LogEventLevel level)
        {
            try
            {
                return func().ContinueWith(x =>
                {
                    if (x.IsFaulted)
                        logger.Write(level, x.Exception, $"{level} swallowed");
                });
            }
            catch (Exception ex)
            {
                logger.Write(level, ex, $"{level} swallowed");
                return Task.CompletedTask;
            }
        }
        
        public static Task Swallow(this ILogger logger, Func<Task> func)
        {
            return SwallowImpl(logger, func, LogEventLevel.Warning);
        }
        
        public static Task SwallowError(this ILogger logger, Func<Task> func)
        {
            return SwallowImpl(logger, func, LogEventLevel.Error);
        }
     
        private static void SwallowImpl(this ILogger logger, Action action, LogEventLevel level)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.Write(level, ex, $"{level} swallowed");
            }
        }
        
        public static void Swallow(this ILogger logger, Action action)
        {
            SwallowImpl(logger, action, LogEventLevel.Warning);
        }
        
        public static void SwallowError(this ILogger logger, Action action)
        {
            SwallowImpl(logger, action, LogEventLevel.Error);
        }
    }
}