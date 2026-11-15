# Enabling Different Tools in Azure AI Foundry with the Agent Framework

The Agent Framework in Azure AI Foundry allows for the extension of an agent's capabilities through various tools. This tutorial will cover how to utilize the Vision, Code Interpreter, Bing Search, and File Search tools with both .NET and Python code samples.

## Portal-created grounded agent with VS Code and C#

This lesson combines the existing file-search and code-interpreter examples in a single agent created in the Microsoft Foundry portal. The portal owns the agent definition and its tool configuration; the C# client connects to that existing Prompt Agent by name.

### Prerequisites and costs

- An Azure subscription with permission and quota to create a Microsoft Foundry project and deploy a supported model.
- Visual Studio Code with the **Foundry Toolkit** extension, the .NET 10 SDK, and Azure CLI.
- Sign in to the same Azure tenant in the portal, VS Code, and Azure CLI (`az login`).
- Allow approximately 45 minutes. Azure model inference, file search, and code interpreter usage may incur charges. Check current regional pricing and quota before creating resources; costs depend on the model, region, and usage.

### Create and ground the agent in the portal

1. Open [Microsoft Foundry](https://ai.azure.com), select **Start building**, and create a project in a region with model quota. Create an agent named `it-support-agent` using an available deployed model.
2. Set its instructions to:

   ```text
   You are an IT Support Agent for Contoso Corporation.
   You help employees with technical issues and IT policy questions.

   Guidelines:
   - Always be professional and helpful
   - Use the IT policy documentation to answer questions accurately
   - If you don't know the answer, admit it and suggest contacting IT support directly
   - When creating tickets, collect all necessary information before proceeding
   ```

3. In **Tools**, add **File search** and **Code interpreter**. Upload [IT_Policy.txt](https://raw.githubusercontent.com/MicrosoftLearning/mslearn-ai-agents/main/Labfiles/01-build-agent-portal-and-vscode/IT_Policy.txt) for file search, then wait for indexing to complete.
4. Add [system_performance.csv](https://raw.githubusercontent.com/MicrosoftLearning/mslearn-ai-agents/main/Labfiles/01-build-agent-portal-and-vscode/system_performance.csv) to the Code interpreter's files and save the agent.
5. In the portal playground, try `What's the policy for password resets?` and `Analyze the system performance data and identify any periods where CPU usage exceeded 80%`. The first should use the policy grounding; the second should analyze the attached CSV. You can also ask for a chart of CPU usage over time.

### Try the agent in VS Code

Install **Foundry Toolkit** from the VS Code Extensions view, sign in, and use **Microsoft Foundry Resources > Set Default Project** to select the project. Expand **Prompt Agents**, open `it-support-agent`, and ask a policy question such as `What is the policy for reporting a lost or stolen device?`. The extension playground uses the same saved agent and grounding.

### Run the C# client

The sample at [`code_samples/dotNET/msfoundry/05-dotnet-agent-framework-msfoundry-portal-agent`](./code_samples/dotNET/msfoundry/05-dotnet-agent-framework-msfoundry-portal-agent/) retrieves the saved Prompt Agent by name; it does not create an agent or configure tools in code.

Set the project endpoint from the Foundry project overview and the agent name in your shell:

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project>"
export FOUNDRY_AGENT_NAME="it-support-agent"
az login
cd 04.Tools/code_samples/dotNET/msfoundry/05-dotnet-agent-framework-msfoundry-portal-agent
dotnet build
dotnet run
```

In PowerShell, use `$env:FOUNDRY_PROJECT_ENDPOINT="..."` and `$env:FOUNDRY_AGENT_NAME="it-support-agent"` in place of `export`. The client uses `DefaultAzureCredential`; Azure CLI authentication is one supported local credential. Running the program sends billable requests to the configured agent. It prints a grounded password-reset answer and an analysis of the uploaded performance data. For more checks, ask the agent to create a chart in the portal or VS Code playground.

### Learning outcomes and parity

| Learning outcome | Demonstration |
| --- | --- |
| Create a Foundry project and Prompt Agent in the portal | Portal steps above |
| Ground an agent with file search and policy data | Attach `IT_Policy.txt` and ask policy questions |
| Enable built-in code interpreter | Attach `system_performance.csv` and request analysis or a chart |
| Explore the saved agent in VS Code | Foundry Toolkit Agent Builder steps above |
| Invoke the portal-created agent programmatically from C# | The `GetAgentAsync` and `RunAsync` calls in the linked sample |
| Agents 05a: publish a grounded agent to Teams and Microsoft 365 Copilot | [Create, publish, and test the enterprise knowledge agent](#agents-05a-publish-a-grounded-agent-to-teams-and-microsoft-365-copilot) |
| Studio 06: explore default and custom guardrails | Follow the Foundry walkthrough below; portal behavior is documented, but live service verification is pending |

This complements the separate tool-creation samples in this chapter: the agent and its tools are configured in the portal, and the sample only connects to and invokes it. The C# proof of concept prints response text; it does not download generated charts or other sandbox files to the local machine. The Foundry Agent Framework type currently emits the `OPENAI001` evaluation-only diagnostic and may change in a future release. Live service behavior depends on an accessible project, deployed model, configured agent, and available regional quota; no live Azure call is part of the build.

### Cleanup

To avoid further charges, delete the project from **Settings > Delete project** in Foundry, or delete its resource group in the Azure portal. Deleting the project also removes the agent and its uploaded data.

## Agents 05a: Publish a grounded agent to Teams and Microsoft 365 Copilot

This lesson follows [Deploy agents to Microsoft Teams and Copilot](https://github.com/hawarren/mslearn-ai-agents-hw/blob/5f58c207928d0dd3963f06066b16bc6c6b0d3ce4/Instructions/Exercises/05a-m365-teams-integration.md). It focuses on publishing and testing a portal-created agent, not building an agent in code. Allow about 40 minutes. A C# client is not included because it does not demonstrate either publishing flow.

### Prerequisites, permissions, and costs

- An Azure subscription, access to the Microsoft Foundry portal, and permission to create a project, deploy or use a model, and create an agent. Model availability and quota vary by region.
- A work or school Microsoft 365 account and a Foundry project role. The documented publisher role is **Foundry User** at the project scope; creating the project or deploying a model can require additional Azure permissions. Teams app access and installation also depend on your tenant's Teams app permission and setup policies.
- Check your tenant's current Microsoft 365 Copilot agent licensing and billing before testing Copilot. A Copilot license is not a universal prerequisite for every agent and tenant configuration; access may instead depend on the user's Microsoft 365 entitlement and pay-as-you-go settings. Do not assume that Teams access alone guarantees Copilot access.
- Organization-wide distribution requires an administrator to approve the request in the Microsoft 365 admin center. The agent author does not need to be that administrator for the direct publish flow. If tenant policy blocks custom apps or individual publishing, ask your Teams or Microsoft 365 administrator to review the applicable policy.
- Foundry model inference and file-search storage/indexing may incur Azure charges. Microsoft 365 licensing or pay-as-you-go Copilot charges are separate. Check current Azure pricing, model quota, and tenant licensing before starting; costs depend on region, model, usage, and tenant plan. Microsoft 365 and Teams also process and store publishing data and agent responses, so confirm the resulting data handling meets your organization's policies.

### Create and ground the agent in Foundry

1. Sign in to [Microsoft Foundry](https://ai.azure.com) with an Azure account that has permission to create a project and use a model. Use the current Foundry experience and create a project in a region with an available model deployment.
2. Create an agent named `enterprise-knowledge-agent` and select an available deployed chat model. Set its instructions to:

   ```text
   You are an Enterprise Knowledge Assistant for Contoso Corporation.

   Your role:
   - Answer questions about company policies and procedures
   - Provide accurate information from uploaded documents
   - Be professional, helpful, and concise
   - If you don't know the answer, say so and suggest who to contact

   Always cite your sources when referencing specific policies.
   ```

3. Save the agent. In its tools or knowledge section, enable file search, upload the [IT security policy](https://raw.githubusercontent.com/MicrosoftLearning/mslearn-ai-agents/main/Labfiles/05a-m365-teams-integration/Python/sample_documents/it_security_policy.txt) and [remote work policy](https://raw.githubusercontent.com/MicrosoftLearning/mslearn-ai-agents/main/Labfiles/05a-m365-teams-integration/Python/sample_documents/remote_work_policy.txt), and wait for indexing to finish.
4. In the Foundry playground, ask `What are the password requirements for my laptop?`, `What are the core hours for remote employees?`, and `What encryption is required on company laptops?`. Verify the responses against the indexed documents and confirm that the agent cites or otherwise identifies its sources. Save the agent after testing.
5. Before publishing, set the active agent version to **Always use latest** or select the specific tested version. The published endpoint uses this active version.

Expected results: passwords require at least 12 characters with uppercase and lowercase letters, numbers, and special characters; remote employees must be available weekdays from 9 AM to 3 PM local time; and BitLocker full-disk encryption is required on company laptops. The wording can vary, but answers should agree with the uploaded policies.

### Publish and test in Teams

1. Open the saved agent and select **Publish** > **Teams and Microsoft Copilot**. Choose direct publishing or download the app package for manual customization/sideloading.
2. Provide the app display name, three-part app version, short and full descriptions, developer information, and HTTPS privacy and terms URLs. Complete app assets as requested by the portal; a custom Teams manifest uses a 192 × 192 color PNG and a 32 × 32 outline PNG. Use URLs and branding approved by your organization, not placeholders.
3. For direct publishing, select **Just you** for personal testing or **People in your organization** for tenant-wide discovery. Submit the app. Foundry registers the Teams bot identity for direct publishing; a separate Azure Bot Service resource is not required. Organization distribution requires Microsoft 365 admin approval.
4. In Teams, find the app under **Your agents** for personal access, or the organization catalog after approval. Install or open it and ask `What are the laptop password requirements?` and `What MFA methods are supported?`. Compare the answers with the Foundry playground. If it does not appear, confirm publishing succeeded and check the tenant's app permission/setup policies.

### Publish and test in Microsoft 365 Copilot

The same publish flow can make the agent available in both Teams and Microsoft 365 Copilot. Copilot access depends on the user's tenant licensing and billing configuration; confirm the current eligibility for your tenant rather than assuming a Copilot add-on license is always required.

1. If the app is not already published, use the saved agent's **Publish** > **Teams and Microsoft Copilot** flow and complete the app details as above.
2. Use **Just you** for a personal test or **People in your organization** for tenant-wide discovery. For organization distribution, an administrator must review and approve the request in the Microsoft 365 admin center before users can find it.
3. Open Microsoft 365 Copilot with an account whose licensing and billing settings allow agent use. Find the agent under **Your agents** for personal access or under **Built by your org** after approval. Start a conversation with the agent or invoke it by name where supported, then ask `What are the core hours for remote employees?` and `What encryption is required on company laptops?`. Check the answers against the policies.
4. If the agent is unavailable, confirm the publishing flow completed, the app was approved if organization-scoped, the user's Copilot licensing/billing supports agent use, and tenant app policies permit access. Copilot surfaces and labels can differ by tenant and may change.

### Outcomes, limitations, and verification status

| Outcome | Status |
| --- | --- |
| Create a portal agent and ground it in two policy documents | Walkthrough documented; indexing and answer quality require a live Foundry project |
| Publish and test the agent in Microsoft Teams | Current portal flow documented; publishing, tenant policy, propagation, and live response have not been tested in this environment |
| Publish and test the agent in Microsoft 365 Copilot | Licensing and approval requirements documented; an eligible tenant and live Copilot test are not available in this environment |
| Avoid source-lab assumptions that may no longer apply | Foundry portal publishing handles Teams bot registration; a separate Azure Bot Service resource is not a prerequisite in the current direct-publishing flow |

Direct publishing is unavailable for projects with public network access disabled; a separate REST publishing path has different network requirements. The direct-publish scopes are personal access (**Just you**) and tenant-wide availability (**People in your organization**), not selection of individual Teams teams or channels. File uploads and image generation in chat are not supported for agents published to Microsoft 365, though the indexed knowledge used by this lesson is configured on the agent. To update agent behavior, change the active version; its stable endpoint does not require republishing. Portal labels and tenant policies can change; see the [Foundry publishing guide](https://learn.microsoft.com/azure/foundry/agents/how-to/publish-copilot), [network and publishing limitations](https://learn.microsoft.com/azure/foundry/agents/how-to/publish-copilot-virtual-network), and confirm licensing and app policies with your administrator. No live tenant checks were possible here, so this parity item is documented but remains pending until publishing and testing succeed in both surfaces.

### Cleanup

Uninstall the app from Teams (**Apps** > **Manage your apps**) and remove or unpublish it from your organization's app catalog if it was shared or approved tenant-wide. Remove the agent from Microsoft 365 Copilot or ask the administrator to remove its organization listing. Then delete the agent and any lab-only project or Azure resources in Foundry. Deleting only the agent/project may not remove every app installation or organization catalog entry; check the Teams and Microsoft 365 admin centers. If you reused a project, resource group, or Foundry resource, remove only assets created for this lesson.

## Studio 06: Explore Foundry guardrails and content filtering

This exercise follows [Apply guardrails to prevent the output of harmful content](https://github.com/hawarren/mslearn-ai-studio-hw/blob/b702dc2fd9f9411b5eb1f86b9259db6515cd9c48/Instructions/Exercises/06-Explore-content-filters.md). Allow about 25 minutes. Content filters can help mitigate harmful prompts and completions, but they are only one part of a responsible-AI approach.

### Prerequisites and costs

- An Azure subscription with permission to create a Microsoft Foundry project/resource and deploy a model, plus available model quota in a supported region.
- Access to the Foundry portal. To run the C# example, install the .NET 10 SDK and Azure CLI, sign in with `az login`, and have a role that permits model inference.
- Model deployments and inference may incur charges. Check current regional model pricing and quota before deploying; usage and costs vary by model and region. Delete the project/resource group when finished.
- The exercise uses `gpt-5.2` as in the source lab. Catalog availability, deployment options, and portal labels can change; if it is unavailable in your region, use an available chat model and note the substitution.

### Create a project and deploy a model

1. Open [Microsoft Foundry](https://ai.azure.com), sign in, and enable **New Foundry** if prompted.
2. Create a uniquely named project. Select your subscription, create or select a resource group, and choose a recommended region with model quota. Wait for the project home page to load.
3. Open **Discover > Models**, find `gpt-5.2` (or the available chat-model substitute noted above), and deploy it using the default settings. When deployment completes, open its playground.

### Explore the default guardrail

1. In the model playground, compare the default guardrail's behavior with the exercise's bank-getaway-planning prompt, an offensive-joke prompt, and `What should I do if I cut myself?`. Do not treat a model's ordinary refusal or self-censoring as proof that a content filter blocked it.
2. Use a safe support-seeking prompt such as `Where can I get help or support related to self-harm?` and record whether a response is returned. Responses can vary by model and policy; the default balanced guardrail may not block every probe.

### Configure and apply custom controls

1. Open **Guardrails** in the project navigation and select **Create**.
2. Under **Add controls**, choose **Risk** and configure **Hate**, **Violence**, **Sexual**, and **Self-harm** with the highest blocking threshold offered by the portal. Add each control. If asked whether to replace an existing control, confirm.
3. Select **Next**, choose **Models**, and apply the guardrail to the deployed model. Review the configuration, submit it, and wait for it to be saved.
4. Open **Deployments**, select the model, and verify on its details page that the custom guardrail is attached. Retry the same probes in the playground and compare results with the default. The custom settings are stricter, but they may not change these particular responses: the model may already refuse them, and outcomes depend on the prompt, model, and current service behavior.

### Handle filtered responses from C#

The optional proof of concept at [`code_samples/dotNET/msfoundry/06-dotnet-foundry-guardrails`](./code_samples/dotNET/msfoundry/06-dotnet-foundry-guardrails/) calls the OpenAI-compatible Chat Completions endpoint for the deployed model. It reports a prompt blocked with `error.code: content_filter` (HTTP 400) and does not print a completion whose `finish_reason` is `content_filter` (HTTP 200). A normal model refusal is not necessarily a filter event. The sample is non-streaming; streaming clients must also stop displaying and remove or replace text if filtering is reported after partial output.

Set the endpoint, deployment name, and a test prompt, then run:

```bash
export FOUNDRY_OPENAI_ENDPOINT="https://<resource>.openai.azure.com/openai/v1"
export FOUNDRY_MODEL="<deployment-name>"
export FOUNDRY_TEST_PROMPT="What should I do if I cut myself?"
az login
cd 04.Tools/code_samples/dotNET/msfoundry/06-dotnet-foundry-guardrails
dotnet build
dotnet run
```

In PowerShell, set the same values with `$env:FOUNDRY_OPENAI_ENDPOINT`, `$env:FOUNDRY_MODEL`, and `$env:FOUNDRY_TEST_PROMPT`. `DefaultAzureCredential` uses the signed-in Azure CLI account locally; the endpoint requires an appropriate model-inference role assignment. Check the returned filter signal in the console and compare it with the playground. This sample demonstrates response handling, not a complete safety system; validate outputs, provide appropriate user guidance, monitor behavior, and use additional responsible-AI controls for a real application.

### Outcomes and verification status

| Outcome | Status |
| --- | --- |
| Test a default model guardrail and compare a custom guardrail with it | Walkthrough documented; requires portal access to demonstrate live behavior |
| Apply four highest-blocking risk controls to a model deployment and verify the association | Walkthrough documented; portal labels and available thresholds may vary |
| Handle prompt- and completion-filtered responses in C# | Implemented and locally buildable; no live Azure inference was run in this environment |
| Treat content filtering as one responsible-AI safeguard | Documented |

Model catalog availability, portal workflows, policy thresholds, and filter outcomes are service-dependent and may change. The prompt examples may be refused by the model itself, and a stricter filter is not guaranteed to change every answer. The parity item remains pending until the portal steps and observed outcomes are demonstrated against an accessible Foundry deployment.

### Cleanup

Delete the model deployment created for this exercise from **Deployments**, then delete any lab-only Foundry project and resource. Alternatively, delete the resource group in the Azure portal if it is dedicated to this exercise. Deleting only the project does not remove model deployments owned by the parent Foundry resource. If you reused a resource group or Foundry resource, remove only the assets created for this exercise.

## 1\. Vision - Using gpt-4o

The Vision tool enables agents to interpret and understand the content of images. This is particularly useful for applications such as identifying objects in a picture, analyzing scenes, and providing contextual information based on visual input. The `gpt-4o` model is leveraged for its advanced multimodal capabilities.

**Python Implementation**

The Python implementation for the Vision tool involves creating an `AzureAIAgentClient` and defining an agent with specific instructions. An image is then loaded, converted to a base64 string, and sent to the agent as part of a `ChatMessage`.

```python
from agent_framework.azure import AzureAIAgentClient
from agent_framework import ChatMessage, DataContent, Role, TextContent
from azure.identity.aio import AzureCliCredential
import os
import base64
from dotenv import load_dotenv

load_dotenv()

AgentName = "Vision-Agent"
AgentInstructions = "You are my furniture sales consultant, you can find different furniture elements from the pictures and give me a purchase suggestion"

async with (
        AzureCliCredential() as credential,
        AzureAIAgentClient(async_credential=credential) as chat_client,
    ):
    
        agent = chat_client.create_agent(
            instructions=(
            AgentName
            ),
            name=AgentInstructions,
        )

        image_path = "../../files/home.png"
        with open(image_path, "rb") as image_file:
            image_b64 = base64.b64encode(image_file.read()).decode()
        image_uri = f"data:image/png;base64,{image_b64}"
        message = ChatMessage(
                role=Role.USER,
                contents=[
                    TextContent(text="Please find the relevant furniture according to the image and give the corresponding price for each piece of furniture"),
                    DataContent(uri=image_uri, media_type="image/png")
                ]
        )
        first_result = await agent.run(message)
        
        print(f"Agent: {first_result.text}")
```

**.NET Implementation**

In the .NET implementation, a `PersistentAgentsClient` is used to create and interact with the agent. The image is read into a byte array and included in the `ChatMessage`.

```csharp
using System;
using System.Linq;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using DotNetEnv;
using System.IO;

Env.Load("../../../../.env");

var azure_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = "gpt-4o";
var imgPath ="../../files/home.png";

const string AgentName = "Vision-Agent";
const string AgentInstructions = "You are my furniture sales consultant, you can find different furniture elements from the pictures and give me a purchase suggestion";

async Task<byte[]> OpenImageBytesAsync(string path)
{
	return await File.ReadAllBytesAsync(path);
}

var imageBytes = await OpenImageBytesAsync(imgPath);

var persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());

var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: azure_foundry_model_id,
    name: AgentName,
    instructions: AgentInstructions);

AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

AgentThread thread = agent.GetNewThread();

// Create the chat message with text and image content
ChatMessage userMessage = new ChatMessage(ChatRole.User, [
	new TextContent("Can you identify the furniture items in this image and suggest which ones would fit well in a modern living room?"), new DataContent(imageBytes, "image/png")
]);

Console.WriteLine(await agent.RunAsync(userMessage, thread));
```

## 2\. Code Interpreter

The Code Interpreter tool allows the agent to write and execute code to perform complex calculations, data analysis, and other programmatic tasks.

**Python Implementation**

The Python example demonstrates how to create an agent with the `HostedCodeInterpreterTool`. The agent is then prompted to use code to solve a problem.

```python
from agent_framework.azure import AzureAIAgentClient
from agent_framework import ChatMessage, DataContent, Role, TextContent,HostedCodeInterpreterTool
from azure.identity.aio import AzureCliCredential
import os
import base64
from dotenv import load_dotenv

load_dotenv()

AgentName = "Coding-Agent"
AgentInstructions = "You are an AI assistant that helps people find information."

async with (
        AzureCliCredential() as credential,
        AzureAIAgentClient(async_credential=credential) as chat_client,
    ):
    
        agent   = chat_client.create_agent(
            instructions=(
            AgentName
            ),
            name=AgentInstructions,
            tools=HostedCodeInterpreterTool(),
        )
        message = ChatMessage(
                role=Role.USER,
                contents=[
                    TextContent(text="Use code to determine the values in the Fibonacci sequence that that are less then the value of 101?")
                ]
        )
        first_result = await agent.run(message)
        
        print(f"Agent: {first_result.text}")
```

**.NET Implementation**

The .NET example creates an agent with a `CodeInterpreterToolDefinition` and then invokes it to solve the same Fibonacci sequence problem.

```csharp
using System;
using System.Linq;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.AI.Agents;
using DotNetEnv;

Env.Load("../../../../.env");

var azure_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1-mini";

const string AgentName = "Code-Agent-Framework";
const string AgentInstructions = "You are an AI assistant that helps people find information.";

var persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());

var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: azure_foundry_model_id,
    name: AgentName,
    instructions: AgentInstructions,
    tools: [new CodeInterpreterToolDefinition()]);

AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

AgentThread thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("Use code to determine the values in the Fibonacci sequence that that are less then the value of 101?", thread));
```

## 3\. Bing Search

The Bing Search tool enables the agent to access the internet to find up-to-date information and answer questions about topics beyond its training data.

**.env Configuration**

To use the Bing Search tool, you need to configure your `.env` file with the necessary credentials. [cite\_start]The `BING_CONNECTION_ID` is a required environment variable[cite: 1].

```
AZURE_AI_PROJECT_ENDPOINT ="Your Azure AI Foundry Project Endpoint"
AZURE_AI_MODEL_DEPLOYMENT_NAME ="Your Azure AI Foundry Project Deployment Name"
BING_CONNECTION_ID="Your Bing Connection ID"
```

**Python Implementation**

The Python implementation for Bing Search uses the `HostedWebSearchTool` to enable the agent to search the web.

```python
from agent_framework.azure import AzureAIAgentClient
from agent_framework import ChatMessage, HostedWebSearchTool,Role, TextContent,HostedCodeInterpreterTool
from azure.identity.aio import AzureCliCredential
import os
import base64
from dotenv import load_dotenv

load_dotenv()

AgentName = "Search-Agent"
AgentInstructions = "You are an AI assistant that helps people find information."

async with (
        AzureCliCredential() as credential,
        AzureAIAgentClient(async_credential=credential) as chat_client,
    ):
    
        agent   = chat_client.create_agent(
            instructions=(
            AgentName
            ),
            name=AgentInstructions,
            tools=[HostedWebSearchTool()],
        )
        message = ChatMessage(
                role=Role.USER,
                contents=[
                    TextContent(text="Could you please describe the workshop according to the link https://github.com/kinfey/GHCAgentWorkshop?")
                ]
        )
        first_result = await agent.run(message)
        
        print(f"Agent: {first_result.text}")
```

**.NET Implementation**

The .NET implementation uses `BingGroundingToolDefinition` to integrate Bing Search capabilities into the agent. The `BING_CONNECTION_ID` is retrieved from the environment variables.

```csharp
using System;
using System.Linq;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using DotNetEnv;

Env.Load("../../../../.env");

var azure_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1-mini";

const string AgentName = "Bing-Agent-Framework";
const string AgentInstructions = "You are an AI assistant that helps people find information.";

var persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());

var conn_id = Environment.GetEnvironmentVariable("BING_CONNECTION_ID");

var bingGroudingConfig = new BingGroundingSearchConfiguration(conn_id);

BingGroundingToolDefinition bingGroundingTool = new(
    new BingGroundingSearchToolParameters(
        [bingGroudingConfig]
    )
);

PersistentAgent agent =await persistentAgentsClient.Administration.CreateAgentAsync(
    model: azure_foundry_model_id,
    name: "bing-search-agent",
    instructions: "Use the bing grounding tool to answer questions.",
    tools: [bingGroundingTool]
);

string agentId = agent.Id;

AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentId);

AgentThread thread = agent.GetNewThread();

ChatMessage userMessage = new ChatMessage(ChatRole.User, [
	new TextContent("Could you please describe the workshop according to the link https://github.com/kinfey/GHCAgentWorkshop?")
]);

var result = await agent.RunAsync(userMessage, thread);
```

## 4\. File Search

The File Search tool allows the agent to search through a collection of documents to find relevant information. This is useful for building retrieval-augmented generation (RAG) applications.

**Python Implementation**

The Python implementation involves creating a vector store, uploading a file, and then creating an agent with the `FileSearchTool`.

```python
import os
from azure.ai.agents.models import FilePurpose,VectorStore,FileSearchTool
from azure.identity.aio import AzureCliCredential
from dotenv import load_dotenv
from agent_framework import AgentRunResponse,ChatAgent,HostedFileSearchTool,HostedVectorStoreContent
from agent_framework.azure import AzureAIAgentClient

load_dotenv()

async def create_vector_store(client: AzureAIAgentClient) -> tuple[str, VectorStore]:
    """Create a vector store with sample documents."""
    file_path = '../../files/demo.md'
    file = await client.project_client.agents.files.upload_and_poll(file_path=file_path, purpose=FilePurpose.AGENTS)
    print(f"Uploaded file, file ID: {file.id}")


    vector_store = await client.project_client.agents.vector_stores.create_and_poll(file_ids=[file.id], name="graph_knowledge_base")

    print(f"Created vector store, ID: {vector_store.id}")


    return file.id, vector_store

async with (
        AzureCliCredential() as credential,
        AzureAIAgentClient(async_credential=credential) as chat_client,
    ):
        file_id, vector_store = await create_vector_store(chat_client)

        file_search = FileSearchTool(vector_store_ids=[vector_store.id])
        
        agent = chat_client.create_agent(
            name="PythonRAGDemo",
            instructions="""
                You are an AI assistant designed to answer user questions using only the information retrieved from the provided document(s).
                """,
            tools=file_search.definitions,
            tool_resources=file_search.resources,
        )
                
        print("Agent created. You can now ask questions about the uploaded document.")

        query = "What's graphrag?"
        response = await AgentRunResponse.from_agent_response_generator(agent.run_stream(query, tool_resources={"file_search": {"vector_store_ids": [vector_store.id]}}))
        print(f"Assistant: {response}")
```

**.NET Implementation**

The .NET implementation involves uploading a file, creating a vector store, and then creating an agent with a `FileSearchToolDefinition`.

```csharp
using System;
using System.Linq;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using DotNetEnv;
using System.IO;

Env.Load("../../../../.env");

var azure_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1-mini";

string pdfPath = "../../files/demo.md";

async Task<Stream> OpenImageStreamAsync(string path)
{
	return await Task.Run(() => File.OpenRead(path));
}

var pdfStream = await OpenImageStreamAsync(pdfPath);

var persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());

PersistentAgentFileInfo fileInfo = await persistentAgentsClient.Files.UploadFileAsync(pdfStream, PersistentAgentFilePurpose.Agents, "demo.md");

PersistentAgentsVectorStore fileStore =
            await persistentAgentsClient.VectorStores.CreateVectorStoreAsync(
                [fileInfo.Id],
                metadata: new Dictionary<string, string>() { { "agentkey", bool.TrueString } });

PersistentAgent agentModel = await persistentAgentsClient.Administration.CreateAgentAsync(
            azure_foundry_model_id,
            name: "GraphRAGAgent",
            tools: [new FileSearchToolDefinition()],
            instructions: "You are an AI assistant that helps people find information in a set of documents. Use the File Search tool to look up relevant information from the files when needed to answer user questions. If you don't know the answer, just say you don't know. Do not make up answers.",
            toolResources: new()
            {
                FileSearch = new()
                {
                    VectorStoreIds = { fileStore.Id },
                }
            },
            metadata: new Dictionary<string, string>() { { "agentkey", bool.TrueString } });

AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentModel.Id);

AgentThread thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("What's graphrag", thread));
```

This tutorial has provided a comprehensive overview of how to enable different tools for your agents in Azure AI Foundry using both .NET and Python. By leveraging these tools, you can significantly enhance the capabilities of your AI agents.