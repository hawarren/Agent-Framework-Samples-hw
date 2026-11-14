using Azure.Identity;
using FoundryResponsesChat;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel.Primitives;

#pragma warning disable OPENAI001

const string Instructions = "You are a helpful AI assistant that answers questions and provides information.";

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME is not set.");
var credential = new DefaultAzureCredential();
var endpointUri = new Uri($"{endpoint.TrimEnd('/')}/openai/v1/");
var tokenPolicy = new BearerTokenPolicy(credential, "https://ai.azure.com/.default");

var openAIClient = new OpenAIClient(tokenPolicy, new OpenAIClientOptions { Endpoint = endpointUri });
var chatClient = openAIClient.GetChatClient(deployment);
var responsesClient = new ResponsesClient(
    tokenPolicy,
    new ResponsesClientOptions { Endpoint = endpointUri });

if (args.Contains("--chat-completions", StringComparer.OrdinalIgnoreCase))
{
    await RunChatCompletionsAsync();
}
else if (args.Contains("--responses", StringComparer.OrdinalIgnoreCase))
{
    await RunResponsesAsync();
}
else if (args.Contains("--async", StringComparer.OrdinalIgnoreCase))
{
    await RunAsyncResponsesAsync();
}
else
{
    await RunStreamingResponsesAsync();
}

async Task RunChatCompletionsAsync()
{
    Console.WriteLine("Chat Completions (no conversation tracking). Enter 'quit' to exit.");
    while (ReadPrompt() is { } prompt)
    {
        ChatCompletion completion = await chatClient.CompleteChatAsync(
        [
            new SystemChatMessage(Instructions),
            new UserChatMessage(prompt)
        ]);
        Console.WriteLine($"Assistant: {completion.Content[0].Text}");
    }
}

async Task RunResponsesAsync()
{
    Console.WriteLine("Responses API (no conversation tracking). Enter 'quit' to exit.");
    while (ReadPrompt() is { } prompt)
    {
        var options = CreateResponseOptions(prompt);
        ResponseResult response = (await responsesClient.CreateResponseAsync(options)).Value;
        Console.WriteLine($"Assistant: {response.GetOutputText()}");
    }
}

async Task RunAsyncResponsesAsync()
{
    var conversation = new ConversationState();
    Console.WriteLine("Async Responses API with conversation tracking. Enter 'quit' to exit.");
    while (ReadPrompt() is { } prompt)
    {
        var options = CreateResponseOptions(prompt, conversation.PreviousResponseId);
        ResponseResult response = (await responsesClient.CreateResponseAsync(options)).Value;
        Console.WriteLine($"Assistant: {response.GetOutputText()}");
        conversation.RecordCompletedResponse(response.Id);
    }
}

async Task RunStreamingResponsesAsync()
{
    var conversation = new ConversationState();
    Console.WriteLine("Streaming Responses API with conversation tracking. Enter 'quit' to exit.");
    while (ReadPrompt() is { } prompt)
    {
        var options = CreateResponseOptions(prompt, conversation.PreviousResponseId);
        options.StreamingEnabled = true;
        await foreach (StreamingResponseUpdate update
            in responsesClient.CreateResponseStreamingAsync(options))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate delta)
            {
                ResponseOutput.WriteDelta(Console.Out, delta.Delta);
            }
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                conversation.RecordCompletedResponse(completed.Response.Id);
            }
        }
        Console.WriteLine();
    }
}

CreateResponseOptions CreateResponseOptions(string prompt, string? previousResponseId = null)
{
    var options = new CreateResponseOptions
    {
        Model = deployment,
        Instructions = Instructions,
        PreviousResponseId = previousResponseId
    };
    options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));
    return options;
}

string? ReadPrompt()
{
    while (true)
    {
        Console.Write("You: ");
        var prompt = Console.ReadLine();
        if (prompt is null || prompt.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }
    }
}

#pragma warning restore OPENAI001
