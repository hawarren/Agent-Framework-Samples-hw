using FoundryResponsesChat;
using Xunit;

namespace FoundryResponsesChatTests;

public class ConversationStateTests
{
    [Fact]
    public void TracksOnlyTheMostRecentlyCompletedResponse()
    {
        var conversation = new ConversationState();

        Assert.Null(conversation.PreviousResponseId);
        conversation.RecordCompletedResponse("response-1");
        conversation.RecordCompletedResponse("response-2");

        Assert.Equal("response-2", conversation.PreviousResponseId);
    }

    [Fact]
    public void ResetStartsANewConversation()
    {
        var conversation = new ConversationState();
        conversation.RecordCompletedResponse("response-1");

        conversation.Reset();

        Assert.Null(conversation.PreviousResponseId);
    }

    [Fact]
    public void WritesTextDeltasWithoutAddingFormatting()
    {
        using var output = new StringWriter();

        ResponseOutput.WriteDelta(output, "hello ");
        ResponseOutput.WriteDelta(output, "world");

        Assert.Equal("hello world", output.ToString());
    }

    [Fact]
    public void IgnoresEmptyTextDeltas()
    {
        using var output = new StringWriter();

        ResponseOutput.WriteDelta(output, null);
        ResponseOutput.WriteDelta(output, string.Empty);

        Assert.Equal(string.Empty, output.ToString());
    }
}
