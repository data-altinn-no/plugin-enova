using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task<T> GetValueAsync<T>(this IDistributedCache distributedCache, string key)
    {
        var encodedPoco = await distributedCache.GetAsync(key);
        if (encodedPoco == null)
        {
            return default;
        }
        var serializedPoco = Encoding.UTF8.GetString(encodedPoco);
        return JsonConvert.DeserializeObject<T>(serializedPoco);
    }

    public static async Task SetValueAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options = null)
    {
        options ??= new DistributedCacheEntryOptions();
        var serializedValue = JsonConvert.SerializeObject(value);
        var  encodedValue = Encoding.UTF8.GetBytes(serializedValue);
        await distributedCache.SetAsync(key, encodedValue, options);
    }
}
