
using Microsoft.Extensions.Caching.Memory;

static void CacheInMemory()
{
    MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
    string key = "sampleKey";
    string value = "Hello, Cache!";

    cache.Set(key, value);
 

    if (cache.TryGetValue(key, out string cachedValue))
    {
        Console.WriteLine($"Cache hit: {cachedValue}");
    }
    else
    {
        Console.WriteLine("Cache miss.");
    }
}


CacheInMemory();
