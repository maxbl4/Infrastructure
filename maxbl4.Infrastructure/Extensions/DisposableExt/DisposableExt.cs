using System;
using Serilog;

namespace maxbl4.Infrastructure.Extensions.DisposableExt
{
    public static class DisposableExt
    {
        public static void DisposeSafe(this IDisposable disposable)
        {
            if (disposable == null) return;
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                var logger = Log.ForContext(disposable.GetType());
                logger.Warning("Dispose failed {ex}", e);
            }
        }
    }
}