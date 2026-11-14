using _06_dotnet_foundry_responses_web_file_search;
using Xunit;

namespace _06_dotnet_foundry_responses_web_file_search.Tests;

public sealed class ConversationTests
{
    [Fact]
    public async Task FollowUpUsesPreviousResponseId()
    {
        FakeResponsesApi api = new(
            new ChatResponse("response-1", "Current events."),
            new ChatResponse("response-2", "Hotels from the brochure."));
        Conversation conversation = new(api);

        await conversation.SendAsync("What's happening in New York next month?");
        string answer = await conversation.SendAsync("Which hotels does Margie's Travel offer there?");

        Assert.Equal("Hotels from the brochure.", answer);
        Assert.Null(api.PreviousResponseIds[0]);
        Assert.Equal("response-1", api.PreviousResponseIds[1]);
        Assert.Equal("response-2", conversation.PreviousResponseId);
    }

    [Fact]
    public async Task ReturnsAssistantOutputText()
    {
        const string expected = "The answer, with the service's response text intact.";
        Conversation conversation = new(new FakeResponsesApi(new ChatResponse("response-1", expected)));

        string answer = await conversation.SendAsync("Tell me about the destination.");

        Assert.Equal(expected, answer);
    }

    private sealed class FakeResponsesApi(params ChatResponse[] responses) : IResponsesApi
    {
        private readonly Queue<ChatResponse> _responses = new(responses);
        public List<string?> PreviousResponseIds { get; } = [];

        public Task<ChatResponse> CreateResponseAsync(string prompt, string? previousResponseId)
        {
            PreviousResponseIds.Add(previousResponseId);
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
