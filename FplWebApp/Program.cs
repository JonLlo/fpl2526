using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles(); // Needed to serve HTML and CSS from wwwroot

app.MapGet("/", async () =>
{
    using HttpClient client = new();

    // Fetch league entries (same as before)
    string leagueUrl = "https://fantasy.premierleague.com/api/leagues-classic/275033/standings/";
    var leagueResponse = await client.GetStringAsync(leagueUrl);
    var leagueJson = JsonDocument.Parse(leagueResponse);

    var entries = leagueJson.RootElement
        .GetProperty("standings")
        .GetProperty("results")
        .EnumerateArray()
        .ToList();

    // Fetch player details
    var playerDetails = new List<(string entryName, int? overallRank)>();
    foreach (var entry in entries)
    {
        int entryId = entry.GetProperty("entry").GetInt32();
        string entryName = entry.GetProperty("entry_name").GetString();

        string playerUrl = $"https://fantasy.premierleague.com/api/entry/{entryId}/";
        var playerResponse = await client.GetStringAsync(playerUrl);
        var playerJson = JsonDocument.Parse(playerResponse);

        int? overallRank = null;
        if (playerJson.RootElement.TryGetProperty("summary_overall_rank", out var rankProp))
        {
            if (rankProp.ValueKind == JsonValueKind.Number)
                overallRank = rankProp.GetInt32();
        }

        playerDetails.Add((entryName, overallRank));
    }

    // Read the HTML file
    string htmlPath = Path.Combine(builder.Environment.WebRootPath, "html", "index.html");
    string htmlTemplate = await File.ReadAllTextAsync(htmlPath);

    // Inject markers
    var rankedPlayers = playerDetails.Where(p => p.overallRank.HasValue).ToList();
    int maxRank = rankedPlayers.Any() ? rankedPlayers.Max(p => p.overallRank.Value) : 1;

    string markersHtml = "";
    foreach (var player in rankedPlayers)
    {
        double leftPercent = (1 - (player.overallRank.Value / (double)maxRank)) * 100;
        markersHtml += $"<div class='marker' style='left: {leftPercent}%;'><span>{player.entryName}</span>|</div>";
    }

    // Inject player list
    string listHtml = "";
    foreach (var player in playerDetails)
    {
        string rankText = player.overallRank.HasValue ? player.overallRank.Value.ToString() : "N/A";
        listHtml += $"<li>{player.entryName}: {rankText}</li>";
    }

    // Replace placeholders in HTML
    htmlTemplate = htmlTemplate.Replace("<!-- Markers will be injected here -->", markersHtml);
    htmlTemplate = htmlTemplate.Replace("<!-- Player list will be injected here -->", listHtml);

    return Results.Content(htmlTemplate, "text/html");
});

app.Run();
