using System.Runtime.CompilerServices;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var calls = new List<(string Name, List<ChatMessage> Messages)>();
AIAgent Fake(string name, string response) =>
    new FakeChatClient(name, response, calls).AsAIAgent(name: name);

var workflow = FeedbackPipeline.Build(
    Fake("summarizer", "Dashboard user requests dark mode."),
    Fake("classifier", "Feature request"),
    Fake("action", "Add dark mode to the product backlog."));
var result = await FeedbackPipeline.RunAsync(workflow, FeedbackPipeline.DefaultFeedback);

Check(calls.Select(call => call.Name).SequenceEqual(["summarizer", "classifier", "action"]), "Agent order");
Check(calls[0].Messages.Any(m => m.Text.Contains("dark mode")), "Feedback reaches summarizer");
Check(calls[1].Messages.Any(m => m.Text == "Dashboard user requests dark mode."), "Summary reaches classifier");
Check(calls[2].Messages.Any(m => m.Text == "Dashboard user requests dark mode."), "Summary reaches action");
Check(calls[2].Messages.Any(m => m.Text == "Feature request"), "Classification reaches action");
Check(result.Select(m => m.Text).SequenceEqual([
    $"Customer feedback: {FeedbackPipeline.DefaultFeedback}",
    "Dashboard user requests dark mode.", "Feature request", "Add dark mode to the product backlog."
]), "Final output contains conversation exactly once");
Check(result.Skip(1).Select(m => m.AuthorName).SequenceEqual(["summarizer", "classifier", "action"]), "Output authors");
var writer = new StringWriter();
FeedbackPipeline.Display(result, writer);
Check(writer.ToString().Contains("01 [user]") && writer.ToString().Contains("04 [action]"), "Display labels");
Check(writer.ToString().Contains("Feature request") && writer.ToString().Contains("Add dark mode to the product backlog."), "Display contents");
FeedbackPipeline.Display(result, Console.Out);

calls.Clear();
var other = await FeedbackPipeline.RunAsync(workflow, "Support resolved my issue quickly.");
Check(calls[0].Messages.Any(m => m.Text.Contains("Support resolved my issue quickly.")), "Custom feedback reaches workflow");
Check(other.Count == 4, "Each run returns its own conversation");
Console.WriteLine("Offline workflow checks passed.");

if (args.Contains("--live"))
{
    var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
        ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT for the live check.");
    var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";
    var client = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());
    var live = await FeedbackPipeline.RunAsync(FeedbackPipeline.BuildFoundry(client, model), FeedbackPipeline.DefaultFeedback);
    FeedbackPipeline.Display(live, Console.Out);
    Check(live.Count == 4 && live.Skip(1).All(m => !string.IsNullOrWhiteSpace(m.Text)), "Live agent outputs");
    Check(live[2].Text.Trim().TrimEnd('.').Equals("Feature request", StringComparison.OrdinalIgnoreCase), "Live classification");
    Console.WriteLine("Live workflow check passed; review the summary and action for quality.");
}

static void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException($"Check failed: {description}");
}

sealed class FakeChatClient(string name, string response, List<(string Name, List<ChatMessage> Messages)> calls) : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        calls.Add((name, messages.ToList()));
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        calls.Add((name, messages.ToList()));
        int midpoint = response.Length / 2;
        yield return new ChatResponseUpdate(ChatRole.Assistant, response[..midpoint]);
        yield return new ChatResponseUpdate(ChatRole.Assistant, response[midpoint..]);
        await Task.CompletedTask;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
