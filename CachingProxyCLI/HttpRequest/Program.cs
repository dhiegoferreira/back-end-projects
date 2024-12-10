// See https://aka.ms/new-console-template for more information


static async Task HttpRequest()
{
    HttpClient client = new HttpClient();
    string url = "https://jsonplaceholder.typicode.com/todos/1";

    HttpResponseMessage response = await client.GetAsync(url);
    string responseBody = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Response from server:");
        Console.WriteLine(responseBody);

}

await HttpRequest();
