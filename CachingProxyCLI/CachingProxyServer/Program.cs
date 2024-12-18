﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

class Program
{
    private static async Task Main(string[] args)
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
        }

        Console.WriteLine($"Port: {port}");
        Console.WriteLine($"Origin: {origin}");

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        Console.WriteLine($"Listening on http://localhost:{port}/");

        MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            string requestUrl = context.Request.RawUrl;
            string cacheKey = $"{origin}{requestUrl}";

            if (cache.TryGetValue(cacheKey, out string cachedResponse))
            {
                Console.WriteLine($"Cache hit: {cacheKey}");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(cachedResponse);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                Console.WriteLine($"Cache miss: {cacheKey}");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"{origin}{requestUrl}");
                string responseBody = await response.Content.ReadAsStringAsync();

                cache.Set(cacheKey, responseBody);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            context.Response.OutputStream.Close();
        }
    }
}