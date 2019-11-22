using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace maxbl4.Infrastructure.Extensions.HttpContentExt
{
    public static class HttpContentExt
    {
        public static async Task<T> ReadAs<T>(this HttpContent content)
        {
            return JsonConvert.DeserializeObject<T>(await content.ReadAsStringAsync());
        }
    }
}