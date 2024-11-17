
using System.Text.Json;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

//Todo
// you could use the city code entered by the user as the key, and save there the result from calling the API.
//At the same time, when you “set” the value in the cache, you can also give it an expiration time in seconds (using the EX flag on the SET command)
//That way the cache (the keys) will automatically clean itself when the data is old enough (for example, giving it a 12-hours expiration time).


var builder = WebApplication.CreateBuilder(args);


string apiKey = builder.Configuration["WeatherApi:ApiKey"];
string redisConnection = builder.Configuration["WeatherApi:RedisConnection"];
        

var redis = builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddHttpClient();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "WeatherAPI";
    config.Title = "WeatherAPI v1";
    config.Version = "v1";
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "WeatherAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
app.UseRateLimiter();


app.MapGet("/weather/{city}", async (string city, DateTime? startDate, DateTime? endDate,IConnectionMultiplexer redis, HttpClient client) =>
        await GetWeatherByCity(city, startDate ?? DateTime.Now, endDate ?? DateTime.Now, apiKey, redis, client)
    ).RequireRateLimiting("fixed");

app.Run();


static async Task<IResult> GetWeatherByCity(string city, DateTime startDate, DateTime endDate,string apiKey, IConnectionMultiplexer redis, HttpClient client)
{

    //Implementation steps
    // Check Cache: Before making a request to the 3rd Party Weather Service, check if the data for the requested city already exists in Redis.
    // Return Cached Response: If the data is found in Redis, return it directly.
    // Request Weather API: If the data is not found in Redis, make a request to the 3rd Party Weather Service.
    // Cache Response: Store the response in Redis for future requests.
    // Return Response: Return the response to the client    

    try
    {
        //Initialize Redis Connection
        var db = redis.GetDatabase();

        // Define a key for storing the data (e.g., "weather:{cityName}")
        string redisKey = $"weather:{city.ToLower()}:{startDate:yyyy-MM-dd}";

        //Step 1: Check cache
        string cachedData = await db.StringGetAsync(redisKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            Console.WriteLine($"Cache hit for {city}");
            var cachedWeather = JsonSerializer.Deserialize<WeatherResponse>(cachedData);
            return TypedResults.Ok(cachedWeather);
        }

        Console.WriteLine($"Cache miss for {city}. Fetching data from 3rd Party API.");

        string url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{city}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}?key={apiKey}";

        //Send a get request
        HttpResponseMessage response = await client.GetAsync(url);

        //Ensure the responde is sucessful (status code 200-299)
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        WeatherResponse weather = JsonSerializer.Deserialize<WeatherResponse>(content);

        string jsonFormatted = JsonSerializer.Serialize(weather);
        await db.StringSetAsync(redisKey, jsonFormatted, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

        Console.WriteLine($"Data for {city} cached with key: {redisKey}");

        return TypedResults.Ok(weather);
    }
    catch (HttpRequestException e)
    {
        return TypedResults.BadRequest(e.Message);
    }
    catch (JsonException e)
    {
        return TypedResults.BadRequest($"JSON Error: {e.Message}");
    }
    catch (RedisException e)
    {
        return TypedResults.BadRequest($"Redis Error:{e.Message}");
    }
}



public class WeatherResponse
{
    [JsonPropertyName("resolvedAddress")]
    public required string ResolvedAddress { get; set; }

    [JsonPropertyName("currentConditions")]
    public required CurrentConditions CurrentConditions { get; set; }

    [JsonPropertyName("days")]
    public required List<Day> Days { get; set; }
}

public class CurrentConditions
{
    

    [JsonPropertyName("temp")]
    public double? Temp { get; set; }

    [JsonPropertyName("humidity")]
    public double? Humidity { get; set; }

    [JsonPropertyName("conditions")]
    public string Conditions { get; set; }


    public CurrentConditions()
    {
        if(Temp != null){
            double aux = (double)Temp;
            Temp = Math.Abs((aux - 32) * (5/9));
        }
    }

}

public class Day
{
    [JsonPropertyName("datetime")]
    public string DateTime { get; set; }

    [JsonPropertyName("tempmax")]
    public double? TempMax { get; set; }

    [JsonPropertyName("tempmin")]
    public double? TempMin { get; set; }

}