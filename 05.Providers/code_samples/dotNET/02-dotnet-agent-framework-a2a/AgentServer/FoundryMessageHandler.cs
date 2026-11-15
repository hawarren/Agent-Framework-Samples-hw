using A2A;

public sealed class FoundryMessageHandler(
    Func<string, CancellationToken, Task<string>> runAgent) : IAgentHandler
{
    public async Task ExecuteAsync(
        RequestContext context,
        AgentEventQueue eventQueue,
        CancellationToken cancellationToken)
    {
        string reply = await HandleMessageAsync(context.UserText ?? string.Empty, cancellationToken);
        var responder = new MessageResponder(eventQueue, context.ContextId);
        await responder.ReplyAsync(reply, cancellationToken: cancellationToken);
    }

    public Task<string> HandleMessageAsync(string message, CancellationToken cancellationToken) =>
        runAgent(message, cancellationToken);
}
