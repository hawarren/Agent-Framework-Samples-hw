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

This complements the separate tool-creation samples in this chapter: the agent and its tools are configured in the portal, and the sample only connects to and invokes it. The C# proof of concept prints response text; it does not download generated charts or other sandbox files to the local machine. The Foundry Agent Framework type currently emits the `OPENAI001` evaluation-only diagnostic and may change in a future release. Live service behavior depends on an accessible project, deployed model, configured agent, and available regional quota; no live Azure call is part of the build.

### Cleanup

To avoid further charges, delete the project from **Settings > Delete project** in Foundry, or delete its resource group in the Azure portal. Deleting the project also removes the agent and its uploaded data.

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