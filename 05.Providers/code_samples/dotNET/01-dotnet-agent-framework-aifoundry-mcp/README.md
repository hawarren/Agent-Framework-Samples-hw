# Foundry agents with remote and custom MCP tools

This .NET proof of concept covers the two MCP paths in the [MCP integration exercise](https://github.com/hawarren/mslearn-ai-agents-hw/blob/5f58c207928d0dd3963f06066b16bc6c6b0d3ce4/Instructions/Exercises/03-mcp-integration.md): a hosted remote Microsoft Learn MCP service and a custom local inventory MCP service. The custom service is read-only and uses in-memory sample data.

## Prerequisites and costs

- Visual Studio Code with Microsoft's **Foundry Toolkit** extension, an Azure subscription, and the .NET 10 SDK.
- Azure CLI signed in to the subscription/project used by the Toolkit (`az login`).
- Permission to create a Foundry project and deploy a model, and model quota in a supported region.
- Allow about 60 minutes for project setup and both walkthroughs.

Creating a model deployment and sending prompts to it can incur Azure charges; check current regional pricing, quota, and deployment billing before proceeding. The Microsoft Learn MCP endpoint is remote, and its request behavior or availability can change. No Azure resource or live endpoint is required to run the local MCP tests.

## Create a project and deploy a model with Foundry Toolkit

1. Install and open **Foundry Toolkit** in VS Code, sign in, and select **Create Project** under **Microsoft Foundry Resources**.
2. Select your subscription/resource group and create a project. In **Models**, open the model catalog and deploy `gpt-4.1` using Global Standard (or Standard if Global Standard is unavailable); leave the default version and token rate unless your quota requires otherwise.
3. Copy the project endpoint from the project in the Toolkit. Set the endpoint and model deployment in the environment:

   ```bash
   export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project>"
   export FOUNDRY_MODEL="gpt-4.1"
   az login
   ```

   In PowerShell, set `$env:FOUNDRY_PROJECT_ENDPOINT` and `$env:FOUNDRY_MODEL` instead. `.env.examples` lists the expected names; export the values in the same shell or configure them in your IDE because the sample does not load `.env` files.

## Run the remote Microsoft Learn MCP agent

From this directory, run:

```bash
dotnet run --project AgentMCP.Console/AgentMCP.Console.csproj -- --remote
```

The client creates a Foundry agent version with the Microsoft Learn MCP tool and requires approval before each MCP call. Review the displayed server, tool, and arguments; enter `Y` to approve. The agent then responds to a prompt for Azure Container Apps commands. The response and number of tool calls can vary. The default mode is also `--remote`.

## Run the custom MCP server, client, and agent

```bash
dotnet run --project AgentMCP.Console/AgentMCP.Console.csproj -- --custom
```

The console starts its own MCP server as a child process over standard input/output, discovers `get_inventory_levels` and `get_weekly_sales`, and exposes the discovered tools to a Foundry-backed Agent Framework agent. The sample inventory includes low-stock Moisturizer and Shampoo with high weekly sales, plus high-stock Body Spray with low weekly sales. The agent should recommend restocking the first two and clearing the last; generated wording can vary. This is a local service demonstration and does not change external inventory.

The server can also be started separately with `--server` for inspection. Set `MCP_SERVER_PROJECT` to the server's project path only if using a non-default layout.

## Focused tests

From this directory:

```bash
dotnet test AgentMCP.Tests/AgentMCP.Tests.csproj
```

The test starts the local server over stdio, checks that both MCP tools are discoverable, and calls them to verify their sample data. It requires the .NET SDK but no Azure login, model deployment, or live MCP endpoint.

## Parity mapping and verification status

| Exercise outcome | This sample | Status |
| --- | --- | --- |
| Create a project and model deployment with Foundry Toolkit | Steps above | Documented; live Toolkit/project setup was not performed in this environment |
| Give an agent access to the remote Microsoft Learn MCP service | `--remote`, with explicit tool approval | Implemented; live Azure and remote-service behavior is opt-in and was not verified here |
| Create custom MCP service tools and connect a client | In-memory inventory server and stdio client | Implemented and covered by the focused local test |
| Let an agent use custom-service tools | `--custom` attaches discovered MCP client tools to the Foundry-backed agent | Implemented; a live model call was not made here |
| Clean up resources | Delete the deployment and lab project/resource group below | Documented |

This sample targets the C# custom server/client path; the Python exercise implementation is not duplicated here. The locally tested protocol path does not prove live Foundry model invocation or remote MCP availability. Keep live-service outcomes pending until they have been exercised with an accessible Foundry project and recorded deployment.

## Cleanup

Stop the console app. In Foundry Toolkit, delete the model deployment created for the walkthrough. Delete the lab-only project or its dedicated resource group when finished; this also removes the hosted agent version created by the remote sample. Do not delete shared resources used by other workloads.
