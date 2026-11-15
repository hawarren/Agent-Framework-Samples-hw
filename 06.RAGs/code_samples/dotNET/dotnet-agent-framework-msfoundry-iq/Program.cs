using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Responses;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your project endpoint.");
string agentName = Environment.GetEnvironmentVariable("FOUNDRY_AGENT_NAME") ?? "product-expert-agent";

var project = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
var agent = (await project.AgentAdministrationClient.GetAgentAsync(agentName)).Value;
#pragma warning disable OPENAI001
var responses = project.ProjectOpenAIClient.GetProjectResponsesClientForAgent(agent.Name);
string? previousResponseId = null;
var history = new List<(string Role, string Text)>();

Console.WriteLine($"Connected to {agent.Name}. Enter a question, 'history', or 'quit'.");
while (true)
{
    Console.Write("You: ");
    string? prompt = Console.ReadLine()?.Trim();
    if (prompt is null || prompt.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;
    if (prompt.Length == 0) continue;
    if (prompt.Equals("history", StringComparison.OrdinalIgnoreCase))
    {
        foreach (var (role, text) in history) Console.WriteLine($"{role}: {text}");
        continue;
    }

    history.Add(("You", prompt));
    var options = new CreateResponseOptions { PreviousResponseId = previousResponseId };
    options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));

    while (true)
    {
        ResponseResult result = await responses.CreateResponseAsync(options);
        previousResponseId = result.Id;
        var approvals = result.OutputItems.OfType<McpToolCallApprovalRequestItem>().ToList();
        if (approvals.Count == 0)
        {
            string answer = result.GetOutputText();
            Console.WriteLine($"Agent: {answer}");
            history.Add(("Agent", answer));
            break;
        }

        options = new CreateResponseOptions { PreviousResponseId = result.Id };
        foreach (var request in approvals)
        {
            Console.WriteLine($"Approval requested: {request.ToolName}\nServer: {request.ServerLabel}\nArguments: {request.ToolArguments}");
            Console.Write("Approve this action? (yes/no): ");
            string? decision = Console.ReadLine();
            if (decision is null) return;
            bool approved = decision.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
                || decision.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
            options.InputItems.Add(ResponseItem.CreateMcpApprovalResponseItem(request.Id, approved));
        }
    }
}
#pragma warning restore OPENAI001
