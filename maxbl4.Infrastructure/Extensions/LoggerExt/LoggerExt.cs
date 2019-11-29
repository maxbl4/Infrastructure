﻿using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace maxbl4.Infrastructure.Extensions.LoggerExt
{
    public static class LoggerExt
    {
        private static Task SwallowImpl(this ILogger logger, Func<Task> func, Action<Exception> onError, LogEventLevel level)
        {
            try
            {
                return func().ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        logger.Write(level, x.Exception, $"{level} swallowed");
                        if (onError != null)
                            logger.SwallowImpl(() => onError(x.Exception), null, level);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Write(level, ex, $"{level} swallowed");
                if (onError != null)
                    logger.SwallowImpl(() => onError(ex), null, level);
                return Task.CompletedTask;
            }
        }
        
        public static Task Swallow(this ILogger logger, Func<Task> func, Action<Exception> onError = null)
        {
            return SwallowImpl(logger, func, onError, LogEventLevel.Warning);
        }
        
        public static Task SwallowError(this ILogger logger, Func<Task> func, Action<Exception> onError = null)
        {
            return SwallowImpl(logger, func, onError, LogEventLevel.Error);
        }
        
        public static Task SwallowFatal(this ILogger logger, Func<Task> func, Action<Exception> onError = null)
        {
            return SwallowImpl(logger, func, onError, LogEventLevel.Fatal);
        }
     
        private static void SwallowImpl(this ILogger logger, Action action, Action<Exception> onError, LogEventLevel level)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.Write(level, ex, $"{level} swallowed");
                if (onError != null)
                    logger.SwallowImpl(() => onError(ex), null, level);
            }
        }
        
        public static void Swallow(this ILogger logger, Action action, Action<Exception> onError = null)
        {
            SwallowImpl(logger, action, onError, LogEventLevel.Warning);
        }
        
        public static void SwallowError(this ILogger logger, Action action, Action<Exception> onError = null)
        {
            SwallowImpl(logger, action, onError, LogEventLevel.Error);
        }
        
        public static void SwallowFatal(this ILogger logger, Action action, Action<Exception> onError = null)
        {
            SwallowImpl(logger, action, onError, LogEventLevel.Fatal);
        }
    }
}