using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

string data = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "data.txt"));
string summary = ExpenseClaim.Summarize(data);
Console.WriteLine($"Here is the expenses data in your file:\n\n{data}");
Console.Write("What would you like me to do with it? ");
string? prompt = Console.ReadLine();
if (string.IsNullOrWhiteSpace(prompt))
    return;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your Foundry project endpoint.");
string model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";

var project = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());
#pragma warning disable OPENAI001
var responses = project.GetProjectOpenAIClient((Azure.AI.Projects.OpenAI.ProjectOpenAIClientOptions?)null).GetProjectResponsesClient();
AIAgent agent = responses.AsAIAgent(
    model: model,
    name: "ExpenseClaimAgent",
    instructions: """
        You assist with expense claims. When the user requests submission, call the
        submit_claim tool with recipient expenses@contoso.com, subject Expense Claim,
        and a body containing all itemized expenses and the supplied total. Do not ask
        for more information. Afterwards confirm that the submission was simulated,
        not actually emailed. For other requests, answer without submitting a claim.
        """,
    tools: [ExpenseClaim.CreateSubmissionTool()]);
#pragma warning restore OPENAI001

Console.WriteLine($"\n# Agent:\n{await agent.RunAsync($"{prompt}\n\nExpenses:\n{summary}")}");
