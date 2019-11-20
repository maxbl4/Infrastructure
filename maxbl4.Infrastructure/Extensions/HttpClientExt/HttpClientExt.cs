using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace maxbl4.Infrastructure.Extensions.HttpClientExt
{
    public static class HttpClientExt
    {
        public static async Task<T> GetAsync<T>(this HttpClient client, string uri)
        {
            var response = await client.GetAsync(uri);
            if (typeof(T) == typeof(string))
                return (T)(object)await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}