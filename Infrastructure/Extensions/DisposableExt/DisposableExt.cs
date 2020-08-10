using System;
using System.Threading.Tasks;
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
        
        public static async Task DisposeAsyncSafe(this IAsyncDisposable disposable)
        {
            if (disposable == null) return;
            try
            {
                await disposable.DisposeAsync();
            }
            catch (Exception e)
            {
                var logger = Log.ForContext(disposable.GetType());
                logger.Warning("DisposeAsync failed {ex}", e);
            }
        }
    }
}