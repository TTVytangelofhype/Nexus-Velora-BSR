using System.Text.Json;
using NexusVeloraBSR.Core.Velora;

namespace NexusVeloraBSR.Bridge;

public sealed class VeloraChatListener : BackgroundService
{
    private readonly IHttpClientFactory _clients;
    private readonly VeloraCommandRouter _router;
    private readonly ILogger<VeloraChatListener> _log;
    private readonly IConfiguration _config;
    private readonly HashSet<string> _seen = new(StringComparer.Ordinal);
    private bool _primed;
    private string? _channelId;

    public VeloraChatListener(IHttpClientFactory clients, VeloraCommandRouter router, ILogger<VeloraChatListener> log, IConfiguration config)
    {
        _clients = clients;
        _router = router;
        _log = log;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _config["NexusVeloraBSR:Channel"] ?? "ttvytangelofhype";
        _log.LogInformation("Velora listener starting for {Channel}", channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _channelId ??= await ResolveChannelIdAsync(channel, stoppingToken);
                if (!string.IsNullOrWhiteSpace(_channelId))
                    await PollAsync(_channelId, stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Velora chat poll failed; retrying.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task<string?> ResolveChannelIdAsync(string channel, CancellationToken ct)
    {
        using var client = _clients.CreateClient("velora");
        using var response = await client.GetAsync("api/users/" + Uri.EscapeDataString(channel), ct);
        if (!response.IsSuccessStatusCode) return channel;

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return FindString(doc.RootElement, "id", "userId", "user_id", "channelId", "channel_id") ?? channel;
    }

    private async Task PollAsync(string channel, CancellationToken ct)
    {
        using var client = _clients.CreateClient("velora");
        using var response = await client.GetAsync("api/chat/channels/" + Uri.EscapeDataString(channel) + "/history?limit=50", ct);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var messages = FindMessageArray(doc.RootElement);
        if (messages == null) return;

        var batch = new List<(string Id, VeloraChatMessage Message)>();
        foreach (var item in messages.Value.EnumerateArray())
        {
            var text = FindString(item, "text", "message", "content", "chatmessage");
            var user = FindString(item, "username", "userName", "name", "displayName", "display_name", "author");
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(user)) continue;

            var id = FindString(item, "id", "messageId", "message_id")
                     ?? $"{user}|{text}|{FindString(item, "timestamp", "createdAt", "created_at")}";
            batch.Add((id, new VeloraChatMessage
            {
                UserName = user,
                Text = text,
                Source = "velora",
                IsBroadcaster = ReadRole(item, "broadcaster", "owner"),
                IsModerator = ReadRole(item, "moderator", "mod")
            }));
        }

        // First poll establishes the watermark. It must not replay old !bsr commands.
        if (!_primed)
        {
            foreach (var entry in batch) Remember(entry.Id);
            _primed = true;
            _log.LogInformation("Velora connected. Watching new chat messages.");
            return;
        }

        foreach (var entry in batch)
        {
            if (!Remember(entry.Id)) continue;
            if (!entry.Message.Text.TrimStart().StartsWith("!", StringComparison.Ordinal)) continue;

            _log.LogInformation("{User}: {Message}", entry.Message.UserName, entry.Message.Text);
            var reply = await _router.HandleAsync(entry.Message);
            if (!string.IsNullOrWhiteSpace(reply))
                _log.LogInformation("NEXUS BSR: {Reply}", reply);
        }
    }

    private bool Remember(string id)
    {
        if (!_seen.Add(id)) return false;
        if (_seen.Count > 500)
        {
            var remove = _seen.Take(_seen.Count - 400).ToArray();
            foreach (var key in remove) _seen.Remove(key);
        }
        return true;
    }

    private static JsonElement? FindMessageArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array) return root;
        foreach (var key in new[] { "messages", "data", "history", "items" })
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Array)
                return value;
        return null;
    }

    private static string? FindString(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        foreach (var name in names)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (!property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
                if (property.Value.ValueKind == JsonValueKind.String) return property.Value.GetString();
                if (property.Value.ValueKind == JsonValueKind.Number) return property.Value.ToString();
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    var nested = FindString(property.Value, "name", "username", "displayName", "id");
                    if (!string.IsNullOrWhiteSpace(nested)) return nested;
                }
            }
        }
        return null;
    }

    private static bool ReadRole(JsonElement item, params string[] roles)
    {
        var role = FindString(item, "role", "userRole", "user_role");
        if (role != null && roles.Any(r => role.Contains(r, StringComparison.OrdinalIgnoreCase))) return true;
        return false;
    }
}
