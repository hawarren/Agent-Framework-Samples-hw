# Remote title and outline agents with A2A

This .NET 10 proof of concept hosts two Microsoft Foundry agents as discoverable A2A services. The router resolves each agent card, asks the title agent for a headline, then sends that exact headline to the outline agent.

## Prerequisites and cost

- .NET 10 SDK and Azure CLI.
- An Azure subscription, a Microsoft Foundry project, and a deployed chat model. The signed-in identity needs permission to use the project and model.
- The project endpoint and model deployment name.

Model inference is billed according to the selected deployment, model, region, and token usage. A provisioned-throughput deployment may incur capacity charges while it exists. Check [Azure AI Foundry pricing](https://azure.microsoft.com/pricing/details/ai-foundry/) before deploying; this sample has no fixed cost estimate.

The sample uses preview versions of the Agent Framework A2A packages. API compatibility may change between releases.

## Configure and run

From the repository root, set the shared configuration and authenticate:

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project>"
$env:FOUNDRY_MODEL = "<model-deployment-name>"
az login
```

In two separate terminals, repeat the Foundry settings and start one agent in each:

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project>"
$env:FOUNDRY_MODEL = "<model-deployment-name>"
$env:AGENT_ROLE = "title"
$env:A2A_PUBLIC_URL = "http://localhost:5001"
dotnet run --project 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/AgentServer/AgentServer.csproj
```

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project>"
$env:FOUNDRY_MODEL = "<model-deployment-name>"
$env:AGENT_ROLE = "outline"
$env:A2A_PUBLIC_URL = "http://localhost:5002"
dotnet run --project 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/AgentServer/AgentServer.csproj
```

The router defaults to those two local URLs. From a third terminal, submit a topic:

```powershell
dotnet run --project 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/Router/Router.csproj -- "React programming"
```

The router should print both discovered agent names, a generated title, and a numbered outline based on that title. You can override the endpoints with `TITLE_AGENT_URL` and `OUTLINE_AGENT_URL`.

The HTTP servers are unauthenticated and intended only for local testing. Do not expose them to an untrusted network.

## Tests and verification

Run the local tests and builds without Azure access:

```powershell
dotnet test 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/Tests/Tests.csproj
dotnet build 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/AgentServer/AgentServer.csproj
dotnet build 05.Providers/code_samples/dotNET/02-dotnet-agent-framework-a2a/Router/Router.csproj
```

For an opt-in live check, run both servers and the router as described above. Confirm the agents' cards are discoverable at `http://localhost:5001/.well-known/agent-card.json` and `http://localhost:5002/.well-known/agent-card.json`, then confirm the outline reflects the title printed by the router.

## Cleanup and parity status

Stop the three local processes with **Ctrl+C**. Delete the lab-only model deployment, Foundry project, and resource group in the Foundry/Azure portal as appropriate. Do not delete shared resources that other workloads use.

| Outcome | Status |
| --- | --- |
| Discoverable title and outline agents; title-to-outline routing over A2A | Both local agent cards smoke-tested; live Foundry route pending |
| Routing and incoming-message unit tests | Passed locally |
| Full exercise parity, including generated title and derived outline from a live model | Pending an opt-in live check |

No Azure project endpoint or credentials were available for this change, so live Foundry inference has not been verified. Do not mark the parity item complete until the live check demonstrates that the outline follows the generated title.
