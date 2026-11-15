using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

public static class FeedbackPipeline
{
    public const string DefaultFeedback = """
        I use the dashboard every day to monitor metrics, and it works well overall.
        But when I'm working late at night, the bright screen is really harsh on my eyes.
        If you added a dark mode option, it would make the experience much more comfortable.
        """;

    public static Workflow Build(AIAgent summarizer, AIAgent classifier, AIAgent action) =>
        AgentWorkflowBuilder.BuildSequential([summarizer, classifier, action]);

    public static Workflow BuildFoundry(AIProjectClient client, string model) =>
        Build(
            client.AsAIAgent(model: model, name: "summarizer",
                instructions: "Summarize the customer's feedback in one short sentence. Keep it neutral and concise."),
            client.AsAIAgent(model: model, name: "classifier",
                instructions: "Classify the feedback as one of the following: Positive, Negative, or Feature request. Return only the classification."),
            client.AsAIAgent(model: model, name: "action",
                instructions: "Based on the summary and classification, suggest the next action in one short sentence."));

    public static async Task<List<ChatMessage>> RunAsync(Workflow workflow, string feedback)
    {
        List<ChatMessage>? output = null;
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
            workflow, new List<ChatMessage> { new(ChatRole.User, $"Customer feedback: {feedback}") });
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent or AgentResponseEvent)
            {
                continue;
            }
            if (evt is WorkflowOutputEvent result)
            {
                output = result.As<List<ChatMessage>>()
                    ?? throw new InvalidOperationException($"Unexpected workflow output: {result.Data?.GetType()}.");
            }
            else if (evt is WorkflowErrorEvent error)
            {
                throw new InvalidOperationException("Workflow failed.", error.Exception);
            }
            else if (evt is ExecutorFailedEvent failed)
            {
                throw new InvalidOperationException($"Executor {failed.ExecutorId} failed: {failed.Data}");
            }
        }

        return output ?? throw new InvalidOperationException("Workflow produced no output.");
    }

    public static void Display(IEnumerable<ChatMessage> messages, TextWriter writer)
    {
        int index = 0;
        foreach (var message in messages)
        {
            string author = message.AuthorName ?? message.Role.ToString();
            writer.WriteLine($"{new string('-', 60)}\n{++index:00} [{author}]\n{message.Text}");
        }
    }
}
