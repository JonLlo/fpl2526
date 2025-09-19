using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles(); // Needed to serve HTML and CSS from wwwroot
app.MapGet("/", async () =>
{
    var html = await System.IO.File.ReadAllTextAsync("wwwroot/index.html");
    return Results.Content(html, "text/html");

});

app.Run();
