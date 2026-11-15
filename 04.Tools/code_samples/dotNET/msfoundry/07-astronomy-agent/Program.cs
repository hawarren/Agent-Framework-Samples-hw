using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
string model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";

var project = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
#pragma warning disable OPENAI001
var agent = project.AsAIAgent(
    model: model,
    name: "astronomy-agent",
    instructions: """
        You are an astronomy observation assistant. Use the tools to find the next event,
        calculate telescope rental costs, and generate observation reports. Ask for missing
        inputs rather than guessing. The events are illustrative sample data.
        """,
    tools: AstronomyTools.CreateTools());
#pragma warning restore OPENAI001

var session = await agent.CreateSessionAsync();
Console.WriteLine("Ask about an event, rental cost, or report. Enter quit to exit.");
while (true)
{
    Console.Write("YOU: ");
    string? prompt = Console.ReadLine();
    if (prompt is null || prompt.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;
    if (string.IsNullOrWhiteSpace(prompt))
        continue;

    Console.WriteLine($"AGENT: {await agent.RunAsync(prompt, session)}");
}
