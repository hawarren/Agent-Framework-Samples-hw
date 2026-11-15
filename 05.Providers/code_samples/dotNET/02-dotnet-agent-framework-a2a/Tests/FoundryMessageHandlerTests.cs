public sealed class FoundryMessageHandlerTests
{
    [Fact]
    public async Task HandleMessageAsyncForwardsIncomingTextAndReturnsAgentReply()
    {
        string? receivedMessage = null;
        var handler = new FoundryMessageHandler((message, _) =>
        {
            receivedMessage = message;
            return Task.FromResult("Generated title");
        });

        string reply = await handler.HandleMessageAsync("Write a title about React.", CancellationToken.None);

        Assert.Equal("Write a title about React.", receivedMessage);
        Assert.Equal("Generated title", reply);
    }
}
