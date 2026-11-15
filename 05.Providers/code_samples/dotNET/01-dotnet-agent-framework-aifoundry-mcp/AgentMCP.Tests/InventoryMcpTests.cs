using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace AgentMCP.Tests;

public class InventoryMcpTests
{
    [Fact]
    public async Task ServerDiscoversAndReturnsInventoryAndSalesTools()
    {
        string projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AgentMCP.Console/AgentMCP.Console.csproj"));
        string configuration = Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))!.Name;

        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Name = "Inventory MCP test",
            Command = "dotnet",
            Arguments = ["run", "--project", projectPath, "--configuration", configuration, "--no-build", "--", "--server"],
            WorkingDirectory = Path.GetDirectoryName(projectPath)
        }));

        var tools = await client.ListToolsAsync();
        Assert.Contains(tools, tool => tool.Name == "get_inventory_levels");
        Assert.Contains(tools, tool => tool.Name == "get_weekly_sales");

        var inventoryResult = await client.CallToolAsync("get_inventory_levels");
        var salesResult = await client.CallToolAsync("get_weekly_sales");
        string inventory = Assert.IsType<TextContentBlock>(Assert.Single(inventoryResult.Content)).Text;
        string sales = Assert.IsType<TextContentBlock>(Assert.Single(salesResult.Content)).Text;

        using JsonDocument inventoryJson = JsonDocument.Parse(inventory);
        using JsonDocument salesJson = JsonDocument.Parse(sales);
        Assert.Equal(6, inventoryJson.RootElement.GetProperty("Moisturizer").GetInt32());
        Assert.Equal(28, inventoryJson.RootElement.GetProperty("Body Spray").GetInt32());
        Assert.Equal(20, salesJson.RootElement.GetProperty("Moisturizer").GetInt32());
        Assert.Equal(3, salesJson.RootElement.GetProperty("Body Spray").GetInt32());
    }
}
