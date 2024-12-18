using System.Net;
using Microsoft.Extensions.Caching.Memory;
internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        int port = 0;
        string origin = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--port" && i + 1 < args.Length)
            {
                port = int.Parse(args[i + 1]);
            }
            else if (args[i] == "--origin" && i + 1 < args.Length)
            {
                origin = args[i + 1];
            } 
            else if (args[i] == "--clear-cache" && i + 1 < args.Length)
            {
                //clear cache
            }
        }

        Console.WriteLine($"Port: {port}");
        Console.WriteLine($"Origin: {origin}");

        //caching-proxy --port <number> --origin <url>

        //Get the port number and origin url

        //Create a new HttpListener and HttpClient        
        HttpListener listener = new HttpListener();




        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        Console.WriteLine($"Listening on http://localhost:{port}/");
        HttpListenerContext context = await listener.GetContextAsync();

        //Before make request to the real server, check if the response is in the cache

        //If the response is in the cache, return the response from the cache   
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
        //If the response is not in the cache, make a request to the real server and cache the response


        HttpClient client = new HttpClient();
        string url = $"{origin}";
        HttpResponseMessage response = await client.GetAsync(url);
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);

    }




}