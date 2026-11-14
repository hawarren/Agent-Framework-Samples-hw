using System.ClientModel;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI;
using OpenAI.Files;
using OpenAI.Responses;
using OpenAI.VectorStores;
using _06_dotnet_foundry_responses_web_file_search;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your Foundry project endpoint.");
string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL to your model deployment name.");

AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());
OpenAIClient openAIClient = projectClient.GetProjectOpenAIClient(
    (Azure.AI.Projects.OpenAI.ProjectOpenAIClientOptions?)null);

OpenAIFileClient fileClient = openAIClient.GetOpenAIFileClient();
string brochurePath = Path.Combine(AppContext.BaseDirectory, "data", "margies-travel-brochure.md");
ClientResult<OpenAIFile> uploadedFile = await fileClient.UploadFileAsync(
    brochurePath,
    FileUploadPurpose.Assistants);
Console.WriteLine($"Uploaded brochure: {uploadedFile.Value.Id}");

VectorStoreClient vectorStoreClient = openAIClient.GetVectorStoreClient();
#pragma warning disable OPENAI001
ClientResult<VectorStore> createdVectorStore = await vectorStoreClient.CreateVectorStoreAsync(
    options: new VectorStoreCreationOptions
    {
        Name = "margies-travel-brochures",
        FileIds = { uploadedFile.Value.Id }
    });
#pragma warning restore OPENAI001
string vectorStoreId = createdVectorStore.Value.Id;
await WaitForIndexingAsync(vectorStoreClient, vectorStoreId);
Console.WriteLine($"Brochure indexed in vector store: {vectorStoreId}");

ResponsesClient responsesClient = openAIClient.GetResponsesClient();
Conversation conversation = new(new FoundryResponsesApi(responsesClient, deploymentName, vectorStoreId));

while (true)
{
    Console.Write("You: ");
    string? prompt = Console.ReadLine();
    if (prompt is null || prompt.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(prompt))
    {
        continue;
    }

    Console.WriteLine($"Assistant: {await conversation.SendAsync(prompt)}");
}

static async Task WaitForIndexingAsync(VectorStoreClient client, string vectorStoreId)
{
    DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(10);
    while (DateTimeOffset.UtcNow < deadline)
    {
        VectorStore vectorStore = (await client.GetVectorStoreAsync(vectorStoreId)).Value;
        string status = vectorStore.Status.ToString();
        if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (status.Equals("expired", StringComparison.OrdinalIgnoreCase)
            || status.Equals("failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Vector store indexing ended with status '{status}'.");
        }

        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    throw new TimeoutException("Timed out waiting for the brochure to be indexed.");
}

sealed class FoundryResponsesApi(
    ResponsesClient client,
    string deploymentName,
    string vectorStoreId) : IResponsesApi
{
    public async Task<ChatResponse> CreateResponseAsync(string prompt, string? previousResponseId)
    {
        CreateResponseOptions options = new()
        {
            Model = deploymentName,
            Instructions = """
                You are a travel assistant for Margie's Travel.
                Use the brochure for Margie's Travel services and hotels.
                Use web search for current or general destination information.
                Keep the two sources distinct and do not invent brochure details.
                """,
            PreviousResponseId = previousResponseId
        };
        options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));
#pragma warning disable OPENAI001
        options.Tools.Add(ResponseTool.CreateWebSearchTool());
        options.Tools.Add(new FileSearchTool([vectorStoreId]));
#pragma warning restore OPENAI001

        ResponseResult response = (await client.CreateResponseAsync(options)).Value;
        return new ChatResponse(response.Id, response.GetOutputText());
    }
}
