using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles(); // Serve HTML and CSS from wwwroot

// Serve the main page
app.MapGet("/", async () =>
{
    var html = await System.IO.File.ReadAllTextAsync("wwwroot/index.html");
    return Results.Content(html, "text/html");
});

// Handle the search form submission
app.MapGet("/search", async (int number) =>
{
    var htmlTemplate = await File.ReadAllTextAsync("wwwroot/index.html");

    string apiUrl = $"https://fantasy.premierleague.com/api/leagues-classic/{number}/standings/";
    using HttpClient client = new();
    string resultsHtml;

    try
    {
        var response = await client.GetStringAsync(apiUrl);
        var jsonDoc = JsonDocument.Parse(response);

        var standings = jsonDoc.RootElement
            .GetProperty("standings")
            .GetProperty("results")
            .EnumerateArray();

        resultsHtml = "<ul>";
        int count = 0;
        foreach (var entry in standings)
        {
            if (count++ >= 22) break;
            string name = entry.GetProperty("entry_name").GetString();
            int rank = entry.GetProperty("rank").GetInt32();
            resultsHtml += $"<li>{rank}: {name}</li>";
        }
        resultsHtml += "</ul>";
    }
    catch
    {
        resultsHtml = $"<p>Could not find league with ID {number}</p>";
    }

    htmlTemplate = htmlTemplate.Replace("@Results", resultsHtml);
    return Results.Content(htmlTemplate, "text/html");
});

app.Run();