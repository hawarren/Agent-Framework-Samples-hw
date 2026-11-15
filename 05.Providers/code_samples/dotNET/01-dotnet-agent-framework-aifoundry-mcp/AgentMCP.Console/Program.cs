using System.ComponentModel;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;
using OpenAI.Responses;

#pragma warning disable OPENAI001

if (args.Contains("--server", StringComparer.OrdinalIgnoreCase))
{
    await RunMcpServerAsync();
}
else if (args.Contains("--custom", StringComparer.OrdinalIgnoreCase))
{
    await RunCustomMcpAgentAsync();
}
else
{
    await RunRemoteMcpAgentAsync();
}

async Task RunRemoteMcpAgentAsync()
{
    string endpoint = GetRequiredSetting("FOUNDRY_PROJECT_ENDPOINT");
    string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";
    var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

    var mcpTool = ResponseTool.CreateMcpTool(
        serverLabel: "microsoft_learn",
        serverUri: new Uri("https://learn.microsoft.com/api/mcp"),
        toolCallApprovalPolicy: new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.AlwaysRequireApproval));

    ProjectsAgentVersion agentVersion = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(
        "MicrosoftLearnAgent",
        new ProjectsAgentVersionCreationOptions(new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = "Answer questions using Microsoft Learn. Use the available MCP tool to find current documentation.",
            Tools = { mcpTool }
        }));

    AIAgent agent = projectClient.AsAIAgent(agentVersion);
    AgentSession session = await agent.CreateSessionAsync();
    AgentResponse response = await agent.RunAsync(
        "Find the Azure CLI commands to create an Azure Container App with a managed identity.",
        session);

    List<ToolApprovalRequestContent> approvalRequests = GetApprovalRequests(response);
    while (approvalRequests.Count > 0)
    {
        List<ChatMessage> approvalResponses = approvalRequests.ConvertAll(approvalRequest =>
        {
            var toolCall = (McpServerToolCallContent)approvalRequest.ToolCall!;
            Console.WriteLine($"""
                The agent requests approval to call an MCP tool. Enter Y to approve:
                Server: {toolCall.ServerName}
                Tool: {toolCall.Name}
                Arguments: {string.Join(", ", toolCall.Arguments?.Select(x => $"{x.Key}: {x.Value}") ?? [])}
                """);
            bool approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
            return new ChatMessage(ChatRole.User, [approvalRequest.CreateResponse(approved)]);
        });

        response = await agent.RunAsync(approvalResponses, session);
        approvalRequests = GetApprovalRequests(response);
    }

    Console.WriteLine($"\nAgent: {response}");
}

#pragma warning restore OPENAI001

async Task RunCustomMcpAgentAsync()
{
    string endpoint = GetRequiredSetting("FOUNDRY_PROJECT_ENDPOINT");
    string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";
    string projectPath = Environment.GetEnvironmentVariable("MCP_SERVER_PROJECT")
        ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../AgentMCP.Console.csproj"));

    await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
    {
        Name = "Inventory MCP server",
        Command = "dotnet",
        Arguments = ["run", "--project", projectPath, "--no-build", "--", "--server"],
        WorkingDirectory = Path.GetDirectoryName(projectPath)
    }));

    var tools = await mcpClient.ListToolsAsync();
    Console.WriteLine($"Connected to the custom MCP server. Tools: {string.Join(", ", tools.Select(tool => tool.Name))}");

    var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
    AIAgent agent = projectClient.AsAIAgent(
        model: deploymentName,
        instructions: """
            You are an inventory assistant. Use the inventory and weekly sales tools to answer.
            Recommend restocking when inventory is below 10 and weekly sales are above 15.
            Recommend clearance when inventory is above 20 and weekly sales are below 5.
            """,
        tools: tools.Cast<AITool>().ToList());

    AgentSession session = await agent.CreateSessionAsync();
    Console.WriteLine(await agent.RunAsync(
        "Show the current inventory and weekly sales for all products, then recommend restocks or clearance.",
        session));
}

async Task RunMcpServerAsync()
{
    var builder = Host.CreateApplicationBuilder();
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();
    await builder.Build().RunAsync();
}

string GetRequiredSetting(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Set {name} to your Foundry project endpoint.");

static List<ToolApprovalRequestContent> GetApprovalRequests(AgentResponse response) =>
    response.Messages.SelectMany(message => message.Contents).OfType<ToolApprovalRequestContent>().ToList();

[McpServerToolType]
public static class InventoryTools
{
    [McpServerTool(Name = "get_inventory_levels", ReadOnly = true, Idempotent = true)]
    [Description("Get the current inventory levels for all products.")]
    public static Dictionary<string, int> GetInventoryLevels() => new()
    {
        ["Moisturizer"] = 6,
        ["Shampoo"] = 8,
        ["Body Spray"] = 28
    };

    [McpServerTool(Name = "get_weekly_sales", ReadOnly = true, Idempotent = true)]
    [Description("Get the units sold this week for all products.")]
    public static Dictionary<string, int> GetWeeklySales() => new()
    {
        ["Moisturizer"] = 20,
        ["Shampoo"] = 18,
        ["Body Spray"] = 3
    };
}
