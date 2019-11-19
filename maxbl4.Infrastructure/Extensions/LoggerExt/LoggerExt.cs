using System;
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
                logger.Error("Error swallowed: {ex}", ex);
            }
        }
    }
}