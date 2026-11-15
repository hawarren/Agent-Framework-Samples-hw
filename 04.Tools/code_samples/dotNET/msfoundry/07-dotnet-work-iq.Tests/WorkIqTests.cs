using System.IO.Pipelines;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

public class WorkIqTests
{
    [Fact]
    public async Task DiscoversAndInvokesOnlyWorkplaceQueryTool()
    {
        Pipe clientToServer = new(), serverToClient = new();
        await using var server = McpServer.Create(
            new StreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream()),
            new McpServerOptions
            {
                ToolCollection =
                [
                    McpServerTool.Create((string question) => $"Result: {question}", new() { Name = "ask" }),
                    McpServerTool.Create(() => "should not run", new() { Name = "delete_entity" })
                ]
            });
        using var cancellation = new CancellationTokenSource();
        var serverTask = server.RunAsync(cancellation.Token);
        await using (var client = await McpClient.CreateAsync(
            new StreamClientTransport(clientToServer.Writer.AsStream(), serverToClient.Reader.AsStream())))
        {
            var tools = WorkIqTools.Select(await client.ListToolsAsync());
            var tool = Assert.Single(tools);
            Assert.Equal("ask", tool.Name);
            var result = await tool.InvokeAsync(new() { ["question"] = "meeting tomorrow" });
            Assert.Contains("Result: meeting tomorrow", result?.ToString());
        }
        cancellation.Cancel();
        await serverTask;
    }

    [Fact]
    public void MissingQueryToolFailsClosed()
    {
        Assert.Throws<InvalidOperationException>(() => WorkIqTools.Select([]));
    }

    [Theory]
    [InlineData("1", "standup", "attendees")]
    [InlineData("2", "website", "blockers")]
    [InlineData("3", "", "this week")]
    [InlineData("3", "today", "today")]
    [InlineData("4", "What changed?", "What changed?")]
    public void ScenariosProduceRelevantPrompts(string choice, string input, string expected)
    {
        Assert.Contains(expected, WorkplacePrompts.Create(choice, input));
    }
}
