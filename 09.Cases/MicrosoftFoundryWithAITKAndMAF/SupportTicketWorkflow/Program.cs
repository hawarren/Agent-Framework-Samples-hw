using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Responses;

#pragma warning disable OPENAI001, AAIP002
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
string name = Environment.GetEnvironmentVariable("FOUNDRY_WORKFLOW_NAME")
    ?? "ContosoPay-Customer-Support-Triage";
string? version = Environment.GetEnvironmentVariable("FOUNDRY_WORKFLOW_VERSION");

var project = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
var conversations = project.ProjectOpenAIClient.GetProjectConversationsClient();
var conversation = (await conversations.CreateProjectConversationAsync()).Value;
try
{
    var responses = project.ProjectOpenAIClient.GetProjectResponsesClientForAgent(
        new AgentReference(name, version), conversation.Id);

    bool finished = false;
    await foreach (var update in responses.CreateResponseStreamingAsync("Start processing support tickets."))
    {
        if (update is StreamingResponseOutputItemDoneUpdate item &&
            item.Item is AgentWorkflowPreviewActionResponseItem action)
        {
            Console.WriteLine($"Action {action.ActionId}: {action.Status}");
        }
        else if (update is StreamingResponseCompletedUpdate completed)
        {
            var response = await responses.GetResponseAsync(completed.Response.Id);
            Console.WriteLine($"\nResponse completed:\n{response.Value.GetOutputText()}");
            finished = true;
        }
        else if (update is StreamingResponseErrorUpdate error)
        {
            throw new InvalidOperationException($"Workflow stream failed: {error.Message}");
        }
    }
    if (!finished)
        throw new InvalidOperationException("Workflow stream ended without a completed response.");
}
finally
{
    await conversations.DeleteConversationAsync(conversation.Id);
}
#pragma warning restore OPENAI001, AAIP002
