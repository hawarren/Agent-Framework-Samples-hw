public sealed class ArticleRouterTests
{
    [Fact]
    public async Task CreateAsyncRoutesGeneratedTitleToOutlineAgent()
    {
        var titleAgent = new FakeAgent("A Practical Guide to React");
        var outlineAgent = new FakeAgent("1. Components\n2. State\n3. Testing");
        var router = new ArticleRouter(titleAgent, outlineAgent);

        ArticleDraft result = await router.CreateAsync("React programming");

        Assert.Equal("React programming", titleAgent.Message);
        Assert.Contains("A Practical Guide to React", outlineAgent.Message);
        Assert.Equal("A Practical Guide to React", result.Title);
        Assert.Equal("1. Components\n2. State\n3. Testing", result.Outline);
    }

    private sealed class FakeAgent(string response) : IRemoteTextAgent
    {
        public string? Message { get; private set; }

        public Task<string> RunAsync(string message, CancellationToken cancellationToken = default)
        {
            Message = message;
            return Task.FromResult(response);
        }
    }
}
