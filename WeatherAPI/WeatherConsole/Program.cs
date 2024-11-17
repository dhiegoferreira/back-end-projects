
using NetTopologySuite.GeometriesGraph;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;


// ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
// IDatabase db = redis.GetDatabase();
//Request example
// https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/London,UK?key=YOUR_API_KEY 

const string _apiKey = "";



try
{

    Console.WriteLine("Typing you location...:");
    string location = Console.ReadLine();
    

    HttpClient client = new();
    string url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{location}/{DateTime.Now.ToString("yyyy-MM-dd")}?key={_apiKey}";



    //Send a get request
    HttpResponseMessage response = await client.GetAsync(url);

    //Ensure the responde is sucessful (statu code 200-299)
    response.EnsureSuccessStatusCode();

    //read the responde content as string
    string content = await response.Content.ReadAsStringAsync();

    //Output the responde content
    System.Console.WriteLine("Response content:");
    System.Console.WriteLine(content);

}
catch (HttpRequestException e)
{
    throw new Exception(e.Message);
}






// db.StringSet("foo", "bar");
// Console.WriteLine(db.StringGet("foo")); 

//Store and retrive a HashMap 
// var hash = new HashEntry[] { 
//     new HashEntry("name", "John"), 
//     new HashEntry("surname", "Smith"),
//     new HashEntry("company", "Redis"),
//     new HashEntry("age", "29"),
//     };
// db.HashSet("user-session:123", hash);

// var hashFields = db.HashGetAll("user-session:123");
// Console.WriteLine(String.Join("; ", hashFields));


// To access Redis Stack capabilities, use the appropriate interface like this:

// IBloomCommands bf = db.BF();
// ICuckooCommands cf = db.CF();
// ICmsCommands cms = db.CMS();
// IGraphCommands graph = db.GRAPH();
// ITopKCommands topk = db.TOPK();
// ITdigestCommands tdigest = db.TDIGEST();
// ISearchCommands ft = db.FT();
// IJsonCommands json = db.JSON();
// ITimeSeriesCommands ts = db.TS();