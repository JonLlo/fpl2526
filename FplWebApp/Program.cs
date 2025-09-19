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

// Handle the form submission
app.MapGet("/search", (int number) =>
{
    // This C# code receives the number
    return $"You entered number: {number}";
});

app.Run();