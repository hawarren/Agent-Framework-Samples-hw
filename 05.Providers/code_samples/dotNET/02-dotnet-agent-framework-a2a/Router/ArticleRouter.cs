using Microsoft.Agents.AI;

public interface IRemoteTextAgent
{
    Task<string> RunAsync(string message, CancellationToken cancellationToken = default);
}

public sealed class A2ARemoteTextAgent(AIAgent agent) : IRemoteTextAgent
{
    public async Task<string> RunAsync(string message, CancellationToken cancellationToken = default) =>
        (await agent.RunAsync(message, cancellationToken: cancellationToken)).Text;
}

public sealed record ArticleDraft(string Title, string Outline);

public sealed class ArticleRouter(IRemoteTextAgent titleAgent, IRemoteTextAgent outlineAgent)
{
    public async Task<ArticleDraft> CreateAsync(string topic, CancellationToken cancellationToken = default)
    {
        string title = await titleAgent.RunAsync(topic, cancellationToken);
        string outlinePrompt =
            $"Create a concise numbered blog post outline for this exact title:\n{title}";
        string outline = await outlineAgent.RunAsync(outlinePrompt, cancellationToken);

        return new ArticleDraft(title, outline);
    }
}
