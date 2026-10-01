using NexusVeloraBSR.Core;
using NexusVeloraBSR.Core.BeatSaver;
using NexusVeloraBSR.Core.Queue;
using NexusVeloraBSR.Core.Velora;
using NexusVeloraBSR.Bridge;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:24842");

builder.Services.AddSingleton<RequestQueue>();
builder.Services.AddSingleton<BeatSaverClient>();
builder.Services.AddSingleton<VeloraCommandRouter>();
builder.Services.AddSingleton<IAcceptedMapHandler, AcceptedMapHandler>();
builder.Services.AddHttpClient("velora", client =>
{
    client.BaseAddress = new Uri("https://api.velora.tv/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("NexusVeloraBSR/0.1.0");
});
builder.Services.AddHostedService<VeloraChatListener>();
builder.Services.AddSingleton<BeatSaberMapInstaller>();
builder.Services.AddSingleton<GameRefreshState>();
builder.Services.AddHttpClient("beatsaver-download", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("NexusVeloraBSR/0.1.0");
});

var app = builder.Build();

var queueSettings = app.Configuration.GetSection("NexusVeloraBSR");
var requestQueue = app.Services.GetRequiredService<RequestQueue>();
var configuredChannel = (queueSettings["Channel"] ?? string.Empty).Trim();
var configuredBeatSaberVersion = (queueSettings["BeatSaberVersion"] ?? "AUTO").Trim();
var displayChannel = string.IsNullOrWhiteSpace(configuredChannel) ? "NOT CONFIGURED" : configuredChannel;
requestQueue.MaxRequestsPerUser = queueSettings.GetValue("MaxRequestsPerUser", 2);
requestQueue.MaxQueueSize = queueSettings.GetValue("MaxQueueSize", 50);
if (!queueSettings.GetValue("RequestsOpen", true)) requestQueue.Close();

app.MapGet("/", () => Results.Ok(new
{
    name = "NEXUS Velora BSR",
    version = "0.1.0",
    status = "online",
    channel = displayChannel,
    beatSaber = configuredBeatSaberVersion
}));

app.MapGet("/api/status", (RequestQueue queue) => Results.Ok(new
{
    status = "online",
    requestsOpen = queue.IsOpen,
    queueCount = queue.Items.Count,
    channel = displayChannel
}));

app.MapGet("/api/queue", (RequestQueue queue) => Results.Ok(queue.Items));

app.MapGet("/api/beatsaber", (BeatSaberMapInstaller installer) =>
{
    var path = installer.FindBeatSaberPath();
    return Results.Ok(new { detected = path != null, path });
});

app.MapGet("/api/game/pending", (GameRefreshState state) => Results.Ok(new { pending = state.Pending, generation = state.Generation }));

app.MapPost("/api/game/refreshed/{generation:long}", (long generation, GameRefreshState state) =>
{
    state.Acknowledge(generation);
    return Results.Ok(new { acknowledged = generation });
});

app.MapPost("/api/install/{key}", async (string key, BeatSaverClient beatSaver, BeatSaberMapInstaller installer, GameRefreshState refreshState, CancellationToken ct) =>
{
    var map = await beatSaver.ResolveAsync(key, ct);
    if (map == null || !string.Equals(map.Key, key, StringComparison.OrdinalIgnoreCase))
        return Results.NotFound(new { error = "BeatSaver map not found." });

    try
    {
        var path = await installer.InstallAsync(map, ct);
        var generation = refreshState.MarkMapInstalled();
        return Results.Ok(new { installed = true, map = map.Name, key = map.Key, path, refreshGeneration = generation });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { installed = false, error = ex.Message });
    }
});

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
Console.WriteLine($"Channel:     {displayChannel}");
Console.WriteLine($"Beat Saber:  {configuredBeatSaberVersion}");
Console.WriteLine("Bridge:      http://127.0.0.1:24842");
Console.WriteLine($"Requests:    {(requestQueue.IsOpen ? "OPEN" : "CLOSED")}");
Console.WriteLine("==========================================");
Console.WriteLine("Waiting for Velora chat requests...");

app.Run();
