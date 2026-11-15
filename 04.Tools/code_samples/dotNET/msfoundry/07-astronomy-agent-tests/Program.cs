using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;
static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var next = Parse(AstronomyTools.NextVisibleEvent("South America", new DateOnly(2026, 4, 30)));
Check(next.GetProperty("event").GetString() == "Jupiter-Venus Conjunction", "Event selection");
Check(next.GetProperty("date").GetString() == "05-01", "Event date");
Check(Parse(AstronomyTools.NextVisibleEvent("south_america", new DateOnly(2026, 5, 1))).GetProperty("event").GetString() == "Jupiter-Venus Conjunction", "Same-day event");
Check(Parse(AstronomyTools.NextVisibleEvent("antarctica", new DateOnly(2026, 12, 31))).TryGetProperty("message", out _), "No wrap to next year");
Check(Parse(AstronomyTools.NextVisibleEvent("unknown", new DateOnly(2026, 1, 1))).TryGetProperty("message", out _), "Unknown location");

var cost = Parse(AstronomyTools.CalculateObservationCost("PREMIUM", 5, "normal"));
Check(cost.GetProperty("total_cost").GetDecimal() == 1875, "Premium cost");
Check(Parse(AstronomyTools.CalculateObservationCost("standard", 1.5m, "urgent")).GetProperty("total_cost").GetDecimal() == 187.5m, "Fractional hours and urgent priority");
Check(Parse(AstronomyTools.CalculateObservationCost("bad", 1, "low")).TryGetProperty("error", out _), "Invalid tier");
Check(Parse(AstronomyTools.CalculateObservationCost("standard", 1, "bad")).TryGetProperty("error", out _), "Invalid priority");
Check(Parse(AstronomyTools.CalculateObservationCost("standard", 0, "low")).TryGetProperty("error", out _), "Invalid hours");

string directory = Path.Combine(Path.GetTempPath(), $"astronomy-test-{Guid.NewGuid():N}");
Directory.CreateDirectory(directory);
string previousDirectory = Environment.CurrentDirectory;
try
{
    Environment.CurrentDirectory = directory;
    Check(Parse(AstronomyTools.GenerateObservationReport("event", "Europe", "bad", 1, "low", "observer")).TryGetProperty("error", out _), "Invalid report booking");
    Check(Directory.GetFiles(directory).Length == 0, "Invalid report must not create a file");

    using var client = new ScriptedClient();
    var agent = client.AsAIAgent(instructions: "Use tools.", tools: AstronomyTools.CreateTools());
    var session = await agent.CreateSessionAsync();
    var response = await agent.RunAsync("Find an event and cost.", session);
    Check(response.ToString().Contains("done"), "Final model response");
    response = await agent.RunAsync("Generate a report for Bellows College.", session);
    Check(response.ToString().Contains("done"), "Follow-up response");
    Check(client.Requests == 4, "Both turns and their tool calls must be processed");
    string[] files = Directory.GetFiles(directory);
    Check(files.Length == 1, "Report tool must create one file");
    string report = File.ReadAllText(files[0]);
    Check(report.Contains("Bellows College") && report.Contains("1875"), "Report content");
}
finally
{
    Environment.CurrentDirectory = previousDirectory;
    Directory.Delete(directory, recursive: true);
}
Console.WriteLine("All astronomy offline checks passed.");

sealed class ScriptedClient : IChatClient
{
    public int Requests { get; private set; }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Requests++;
        if (Requests == 1)
        {
            if (options?.Tools?.Count != 3) throw new Exception("Three tools must be registered");
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
            [
                new FunctionCallContent("event-1", "next_visible_event", new Dictionary<string, object?> { ["location"] = "south_america" }),
                new FunctionCallContent("cost-1", "calculate_observation_cost", new Dictionary<string, object?> { ["telescope_tier"] = "premium", ["hours"] = 5, ["priority"] = "normal" })
            ])));
        }

        var results = messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().ToArray();
        if (Requests == 2)
        {
            if (!results.Any(r => r.CallId == "event-1") ||
                !results.Any(r => r.CallId == "cost-1" && r.Result?.ToString()?.Contains("1875") == true))
                throw new Exception("Event and cost outputs must be returned with their call IDs");
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "done")));
        }
        if (Requests == 3)
        {
            if (!messages.Any(m => m.Role == ChatRole.User && m.Text?.Contains("Find an event") == true))
                throw new Exception("Follow-up must retain conversation history");
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
            [
                new FunctionCallContent("report-1", "generate_observation_report", new Dictionary<string, object?>
                {
                    ["event_name"] = "Jupiter-Venus Conjunction", ["location"] = "south_america",
                    ["telescope_tier"] = "premium", ["hours"] = 5, ["priority"] = "normal",
                    ["observer_name"] = "Bellows College"
                })
            ])));
        }

        if (!results.Any(r => r.CallId == "report-1" && r.Result?.ToString()?.Contains("Report generated") == true))
            throw new Exception("Report output must be returned with its call ID");
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "done")));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
