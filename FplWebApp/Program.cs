using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    using HttpClient client = new();

    // 1️⃣ Get league entries
    string leagueUrl = "https://fantasy.premierleague.com/api/leagues-classic/275033/standings/";
    var leagueResponse = await client.GetStringAsync(leagueUrl);
    var leagueJson = JsonDocument.Parse(leagueResponse);

    var entries = leagueJson.RootElement
        .GetProperty("standings")
        .GetProperty("results")
        .EnumerateArray()
        .ToList(); // materialize for multiple loops

    // 2️⃣ Fetch player details for rank
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
    // else leave as null
}

        playerDetails.Add((entryName, overallRank));
    }

    // 3️⃣ Build HTML
    var html = @"
    <html>
      <body>
        <h1>League Entries</h1>
        <div style='position: relative; width: 100%; height: 50px; border-top: 2px solid black;'>
    ";

    


  // Determine max rank for scaling (ignoring nulls)
    var rankedPlayers = playerDetails.Where(p => p.overallRank.HasValue).ToList();
    int maxRank = rankedPlayers.Any() ? rankedPlayers.Max(p => p.overallRank.Value) : 1; // default 1 if none

  foreach (var player in rankedPlayers)
{
int left = (int)((1 - (player.overallRank.Value / (double)maxRank)) * 1200);
    html += $"<div style='position: absolute; left: {left}px; top: -10px;'>|<br/>{player.entryName}</div>";
}

    html += "</div>";

    // Player list with ranks
    html += "<ul>";
    foreach (var player in playerDetails)
    {
        string rankText = player.overallRank.HasValue ? player.overallRank.Value.ToString() : "N/sA";
        html += $"<li>{player.entryName}: {rankText}</li>";
    }
    html += "</ul></body></html>";

    return Results.Content(html, "text/html");
});

app.Run();
