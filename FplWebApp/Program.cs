







using System.Net.Http;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    using HttpClient client = new();
    string leagueUrl = "https://fantasy.premierleague.com/api/leagues-classic/275041/standings/";
    var response = await client.GetStringAsync(leagueUrl);

    var jsonDoc = JsonDocument.Parse(response);
    var entries = jsonDoc.RootElement
        .GetProperty("standings")
        .GetProperty("results")
        .EnumerateArray();

    var html = "<html><body><h1>League Entries</h1><ul>";

    foreach (var entry in entries)
    {
        int entryId = entry.GetProperty("entry").GetInt32();
        string playerName = entry.GetProperty("player_name").GetString();

        html += $"<li>{entryId}: {playerName}</li>";
    }

    html += "</ul></body></html>";

    // Return HTML content with content-type header
    return Results.Content(html, "text/html");
});

app.Run();
