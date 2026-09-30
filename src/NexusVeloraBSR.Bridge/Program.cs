using NexusVeloraBSR.Core.BeatSaver;
using NexusVeloraBSR.Core.Queue;
using NexusVeloraBSR.Core.Velora;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:24842");

builder.Services.AddSingleton<RequestQueue>();
builder.Services.AddSingleton<BeatSaverClient>();
builder.Services.AddSingleton<VeloraCommandRouter>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "NEXUS Velora BSR",
    version = "0.1.0",
    status = "online",
    channel = "ttvytangelofhype",
    beatSaber = "1.42.1"
}));

app.MapGet("/api/status", (RequestQueue queue) => Results.Ok(new
{
    status = "online",
    requestsOpen = queue.IsOpen,
    queueCount = queue.Items.Count,
    channel = "ttvytangelofhype"
}));

app.MapGet("/api/queue", (RequestQueue queue) => Results.Ok(queue.Items));

app.MapPost("/api/chat", async (VeloraChatMessage message, VeloraCommandRouter router) =>
{
    if (string.IsNullOrWhiteSpace(message.UserName) || string.IsNullOrWhiteSpace(message.Text))
        return Results.BadRequest(new { error = "userName and text are required" });

    var reply = await router.HandleAsync(message);
    return Results.Ok(new { handled = reply != null, reply });
});

Console.WriteLine("==========================================");
Console.WriteLine("       NEXUS VELORA BSR v0.1.0");
Console.WriteLine("==========================================");
Console.WriteLine("Channel:     ttvytangelofhype");
Console.WriteLine("Beat Saber:  1.42.1");
Console.WriteLine("Bridge:      http://127.0.0.1:24842");
Console.WriteLine("Requests:    OPEN");
Console.WriteLine("==========================================");
Console.WriteLine("Waiting for Velora chat requests...");

app.Run();
