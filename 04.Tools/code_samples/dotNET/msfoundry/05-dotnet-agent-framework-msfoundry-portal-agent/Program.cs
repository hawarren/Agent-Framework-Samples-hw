using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your Foundry project endpoint.");
string agentName = Environment.GetEnvironmentVariable("FOUNDRY_AGENT_NAME") ?? "it-support-agent";

var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
var agentRecord = await projectClient.AgentAdministrationClient.GetAgentAsync(agentName);
#pragma warning disable OPENAI001
FoundryAgent agent = projectClient.AsAIAgent(agentRecord);
#pragma warning restore OPENAI001
var session = await agent.CreateSessionAsync();

string[] prompts =
[
    "What's the policy for password resets?",
    "Analyze the system performance data and identify any periods where CPU usage exceeded 80%."
];

foreach (string prompt in prompts)
{
    Console.WriteLine($"You: {prompt}");
    Console.WriteLine($"Agent: {await agent.RunAsync(prompt, session)}");
}
