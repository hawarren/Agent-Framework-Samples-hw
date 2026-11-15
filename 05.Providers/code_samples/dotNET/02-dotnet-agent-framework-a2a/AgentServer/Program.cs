using A2A;
using A2A.AspNetCore;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var builder = WebApplication.CreateBuilder(args);

string role = Environment.GetEnvironmentVariable("AGENT_ROLE") ?? "title";
string publicUrl = Environment.GetEnvironmentVariable("A2A_PUBLIC_URL")
    ?? (role == "outline" ? "http://localhost:5002" : "http://localhost:5001");
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your Foundry project endpoint.");
string model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL to your deployed model name.");

var (name, description, instructions, skillId, skillName, skillDescription, skillTag) = role switch
{
    "title" => (
        "Title Agent",
        "Generates a clear, catchy blog title from a topic.",
        "Given a topic, suggest one clear and catchy blog post title. Return only the title.",
        "generate-title",
        "Generate a blog title",
        "Creates a blog title from a topic.",
        "title"),
    "outline" => (
        "Outline Agent",
        "Creates a concise blog outline from a supplied title.",
        "Create a concise numbered blog post outline for the supplied title. Keep the title unchanged.",
        "generate-outline",
        "Generate a blog outline",
        "Creates an article outline based on a title.",
        "outline"),
    _ => throw new InvalidOperationException("Set AGENT_ROLE to either 'title' or 'outline'.")
};

AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());
#pragma warning disable OPENAI001
AIAgent agent = projectClient.AsAIAgent(model: model, instructions: instructions);
#pragma warning restore OPENAI001

var card = new AgentCard
{
    Name = name,
    Description = description,
    Version = "1.0.0",
    SupportedInterfaces =
    [
        new AgentInterface
        {
            Url = $"{publicUrl.TrimEnd('/')}/a2a",
            ProtocolBinding = "JSONRPC",
            ProtocolVersion = "1.0"
        }
    ],
    DefaultInputModes = ["text/plain"],
    DefaultOutputModes = ["text/plain"],
    Capabilities = new AgentCapabilities { Streaming = false },
    Skills =
    [
        new A2A.AgentSkill
        {
            Id = skillId,
            Name = skillName,
            Description = skillDescription,
            Tags = [skillTag]
        }
    ]
};

builder.WebHost.UseUrls(publicUrl);
builder.Services.AddSingleton<Func<string, CancellationToken, Task<string>>>(
    async (message, cancellationToken) =>
        (await agent.RunAsync(message, cancellationToken: cancellationToken)).Text);
builder.Services.AddA2AAgent<FoundryMessageHandler>(card);

var app = builder.Build();
app.MapA2A("/a2a");
app.MapWellKnownAgentCard(card);
app.Run();
