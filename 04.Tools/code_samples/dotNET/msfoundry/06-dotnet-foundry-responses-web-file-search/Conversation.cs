namespace _06_dotnet_foundry_responses_web_file_search;

public sealed record ChatResponse(string Id, string Text);

public interface IResponsesApi
{
    Task<ChatResponse> CreateResponseAsync(string prompt, string? previousResponseId);
}

public sealed class Conversation(IResponsesApi responsesApi)
{
    public string? PreviousResponseId { get; private set; }

    public async Task<string> SendAsync(string prompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ChatResponse response = await responsesApi.CreateResponseAsync(prompt, PreviousResponseId);
        PreviousResponseId = response.Id;
        return response.Text;
    }
}
