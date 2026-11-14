namespace FoundryResponsesChat;

public sealed class ConversationState
{
    public string? PreviousResponseId { get; private set; }

    public void RecordCompletedResponse(string responseId)
    {
        PreviousResponseId = responseId;
    }

    public void Reset()
    {
        PreviousResponseId = null;
    }
}

public static class ResponseOutput
{
    public static void WriteDelta(TextWriter output, string? delta)
    {
        if (!string.IsNullOrEmpty(delta))
        {
            output.Write(delta);
        }
    }
}
