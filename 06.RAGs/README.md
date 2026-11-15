# Tutorial: Building an Agent with RAG Functionality

This tutorial will guide you through building a Retrieval-Augmented Generation (RAG) agent using both .NET (C\#) and Python with Azure AI. We will cover the basic concepts of RAG and then walk through code examples that demonstrate how to implement an agent that can answer questions based on a provided document.

## 1\. Understanding RAG

### What is RAG?

Retrieval-Augmented Generation (RAG) is a technique that enhances the capabilities of Large Language Models (LLMs) by grounding them in external sources of knowledge. It combines two main components:

  * **Retriever:** This component is responsible for searching and retrieving relevant information from a knowledge base (e.g., a collection of documents).
  * **Generator:** This is a standard LLM that takes the retrieved information as context and uses it to generate a comprehensive and accurate answer to the user's query.

By providing the LLM with relevant, up-to-date information, RAG helps to reduce hallucinations (making up facts) and allows the model to answer questions about specific, private data it wasn't trained on.

### RAG Application Scenarios

RAG is highly effective in various scenarios, including:

  * **Customer Support:** Building chatbots that can answer customer questions based on product manuals, FAQs, and internal knowledge bases.
  * **Enterprise Search:** Creating intelligent search systems that allow employees to ask questions and get precise answers from a vast repository of internal documents.
  * **Content Discovery:** Assisting users in finding and understanding information within large, complex documents like research papers, legal contracts, or financial reports.

### Creating a RAG Agent with File Search

The fundamental workflow for creating a RAG agent with a file search capability involves these steps:

1.  **Upload Knowledge:** Provide the documents that the agent will use as its knowledge base.
2.  **Create a Vector Store:** The uploaded files are indexed into a specialized database called a vector store. This allows for efficient semantic searching to find the most relevant document snippets for a given query.
3.  **Define the Agent:** Create an agent and equip it with a `File Search` tool.
4.  **Link Resources:** Connect the `File Search` tool to the vector store you created.
5.  **Instruct the Agent:** Provide a clear system prompt that instructs the agent on how to behave—for example, telling it to only use the provided files to answer questions and to admit when it doesn't know the answer.
6.  **Query the Agent:** Start a conversation and ask questions. The agent will automatically use the file search tool to retrieve context before generating its response.

-----

## Foundry IQ: Contoso product agent (Agents 04)

This is the C# counterpart to [Integrate an AI agent with Foundry IQ](https://github.com/hawarren/mslearn-ai-agents-hw/blob/5f58c207928d0dd3963f06066b16bc6c6b0d3ce4/Instructions/Exercises/04-integrate-agent-with-foundry-iq.md). Unlike the file-search example below, Foundry IQ uses an Azure AI Search knowledge base and a portal-configured agent.

### Prerequisites and costs

- An Azure subscription with permission to create a Foundry project and model deployments, AI Search, and a storage account, and to configure their connections. Granting roles may require additional permissions. The signed-in client identity needs access to the project and agent (for example, **Azure AI User** on the project). See [Foundry environment setup](https://learn.microsoft.com/azure/foundry/agents/environment-setup) and [Foundry IQ connection guidance](https://learn.microsoft.com/azure/foundry/agents/how-to/foundry-iq-connect) for current roles and managed-identity requirements.
- A region supporting Foundry IQ/agentic retrieval and the selected chat and embedding models, with sufficient model quota. Portal labels and preview availability can change; see [region support](https://learn.microsoft.com/azure/search/search-region-support).
- .NET 10 SDK and Azure CLI. Sign in with `az login` (and `az account set --subscription <subscription-id>` if necessary); the client uses `DefaultAzureCredential`. Set the project endpoint, not an Azure OpenAI deployment endpoint.
- Search, storage, indexing, agentic retrieval, and model inference can incur charges. Free Search may not be available; Basic is paid. Check [agentic retrieval billing](https://learn.microsoft.com/azure/search/agentic-retrieval-how-to-enable-disable) and your subscription's pricing before provisioning. Preview features can change and have no production SLA.

### Set up the agent and knowledge base

1. In [Foundry](https://ai.azure.com), enable **New Foundry**, create a project such as `agent-iq-lab` in your subscription and resource group, and note its project endpoint. Under **Build > Agents**, create `product-expert-agent` with an available chat model such as `gpt-4.1`. Set and save these instructions:

   ```text
   You are a helpful AI assistant for Contoso, specializing in outdoor camping and hiking products.
   You must ALWAYS search the knowledge base to answer questions about our products or product
   catalog. Provide detailed, accurate information and always cite your sources.
   If you don't find relevant information in the knowledge base, say so clearly.
   ```

2. Under **Knowledge > Add > Connect to Foundry IQ**, choose **Connect to an AI Search resource > Create new resource**. Use a globally unique name, the project's subscription, resource group and region, and Free if available or Basic otherwise. The pinned exercise lists the Foundry IQ knowledge-base capability setting as “Pause til next month”; verify the current portal's billing and capability options before choosing. Wait for creation.
3. Download [the Contoso product ZIP](https://github.com/MicrosoftLearning/mslearn-ai-agents/raw/main/Labfiles/04-integrate-agent-with-foundry-iq/data/contoso-products.zip) and extract its three PDFs. In the [Azure portal](https://portal.azure.com), create a uniquely named storage account in the same subscription, group and region: Blob Storage or Data Lake Storage, Standard performance, locally redundant (LRS). Create the `contosoproducts` container and upload all three PDFs.
4. On the Search service's **Security + networking > Keys** page, set API access control to **Both** and confirm. In Foundry refresh **Knowledge**, choose **Create a knowledge base > Azure Blob Storage > Connect**, and configure:
   - Knowledge source `ks-contosoproducts`, description `Contoso product catalog items`, the storage account and `contosoproducts` container.
   - Authentication **API Key**, content extraction **minimal**, an available embedding deployment such as `text-embedding-3-small`, and a chat deployment such as `gpt-4.1`.
   - Create the source, select the chat model on the knowledge-base page, keep the remaining defaults, and **Save knowledge base**. Refresh until the source is **active**.
5. On **Knowledge**, choose **Manage** beside the connection selector. Under **Connected resources**, select the Search service, **Edit authentication**, and save a Search key from its Azure portal **Keys** page. Never put keys in source code or terminal history. This follows the exercise's key-based setup; prefer managed identity and least privilege where supported. Key-based shared access does not enforce per-user document permissions.
6. Return to **Build > Agents**, open the agent, and add Foundry IQ in its knowledge section, selecting the Search connection and new knowledge base. Save the agent. If configurable, require MCP tool-call approval; approval behavior depends on the portal tool configuration. Approval is separate from Azure authorization.
7. In the playground, ask `What types of tents does Contoso offer?`, `Tell me about which backpacks are available in XL.`, and `What camping accessories are available?` Check for specific catalog information and source references. Optionally try **Preview agent**. If the source is not active or retrieval fails, check indexing, region, model quota, connection authentication and permissions before continuing.

### Run the C# client

From the repository root, using your own project endpoint and agent name:

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project>"
export FOUNDRY_AGENT_NAME="product-expert-agent"
dotnet build 06.RAGs/code_samples/dotNET/dotnet-agent-framework-msfoundry-iq
dotnet run --project 06.RAGs/code_samples/dotNET/dotnet-agent-framework-msfoundry-iq
```

The client retrieves the existing agent by name and calls the Foundry Responses SDK directly, rather than creating a file-search agent. It chains response IDs for context instead of creating the exercise's explicit conversation object. It displays each requested MCP tool, server and arguments; only `yes` or `y` approves, and other input denies. End-of-input exits without submitting an approval. It handles multiple requests and further approval rounds. `history` displays local user/assistant text and `quit` exits; history is not persisted locally. Service-side responses may remain stored according to service retention settings.

### Opt-in live validation

With the resources configured and billing understood, run the client interactively and ask these in order:

1. `What types of outdoor products does Contoso offer?` — approve a search and check for information from the product documents.
2. `Tell me about the weatherproof features of your tents.` — approve and check the tent catalog.
3. `What's the difference between your daypacks and expedition backpacks?` — approve and check the backpack guide.
4. `What camping accessories would you recommend for a weekend hiking trip?` — approve and check the recommendations against the documents.
5. `How much do those items typically cost?` — check that “those items” refers to the previous answer. Then enter `history` and `quit`.

Also deny a requested search and verify it is not executed, and ask about a product not in the documents to check the agent acknowledges missing evidence. Approvals only appear when the configured tool requires them; do not enable automatic approval to make this check pass. Inspect source references/citations where returned and compare answers with the PDFs. Model answers and citation formatting are not deterministic; text output may not expose every structured citation. If no approval or citation appears, inspect the agent configuration and service response rather than assuming success.

### Exercise parity and limitations

| Exercise outcome | This sample | Status |
| --- | --- | --- |
| Project, Search, Blob data, knowledge source and portal agent | Setup above | Pending live verification |
| Playground retrieval and source references | Three portal queries above | Pending live verification |
| C# retrieval, approval/denial and conversation context | Client and opt-in queries above | Builds locally; pending live verification |
| History, missing-evidence behavior and cleanup | Client and instructions above | Pending live verification |

No Azure deployment was available for this change, so the portal flow, retrieval, citations, approval settings and live outcomes have **not** been demonstrated; parity is not complete. The SDK uses a response chain instead of the Python exercise's explicit conversation and only prints returned text, not all structured citation metadata. Runtime/service errors are surfaced to the console rather than automatically retried.

### Cleanup

If the resource group was created solely for this lab, delete it in the Azure portal after checking it contains no shared resources. Otherwise delete only the lab agent, connections, knowledge base and source, Search service, model deployments, storage container/account and any other lab resources you created. Deleting the agent or connection alone does not delete Search or storage. Review remaining resources and charges.

-----

## 2\. Code Examples

The following examples demonstrate how to build a RAG agent using the Azure AI Agent Framework in both .NET and Python. Both examples will use a file named `demo.md` as the knowledge source.

### .NET (C\#) Example

This example uses a C\# Polyglot Notebook to create and interact with the RAG agent.

**Step 1: Setup and Dependencies**

First, we need to reference the necessary NuGet packages for the Azure AI Agent Framework, Azure identity, and environment variable management.

```csharp
#r "nuget: Microsoft.Extensions.AI, 9.9.0"
#r "nuget: Azure.AI.Agents.Persistent, 1.2.0-beta.5"
#r "nuget: Azure.Identity, 1.15.0"
#r "nuget: System.Linq.Async, 6.0.3"
#r "nuget: DotNetEnv, 3.1.1"
```

**Step 2: Load Configuration and Initialize Client**

Load environment variables from a `.env` file and create the `PersistentAgentsClient` using your Azure credentials.

```csharp
using DotNetEnv;
using Azure.AI.Agents.Persistent;
using Azure.Identity;

// Load environment variables
Env.Load("../../../.env");
var azure_foundry_endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_FOUNDRY_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = Environment.GetEnvironmentVariable("FOUNDRY_MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1-mini";

// Initialize the client
var persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());
```

**Step 3: Upload File and Create Vector Store**

Upload the `demo.md` file and use its ID to create a vector store. This makes the file's content searchable.

```csharp
// Path to the local file
string pdfPath = "../files/demo.md";
var pdfStream = await Task.Run(() => File.OpenRead(pdfPath));

// Upload the file
PersistentAgentFileInfo fileInfo = await persistentAgentsClient.Files.UploadFileAsync(pdfStream, PersistentAgentFilePurpose.Agents, "demo.md");

// Create the vector store
PersistentAgentsVectorStore fileStore =
            await persistentAgentsClient.VectorStores.CreateVectorStoreAsync(
                [fileInfo.Id],
                metadata: new Dictionary<string, string>() { { "agentkey", bool.TrueString } });
```

**Step 4: Create the RAG Agent**

Define a new persistent agent. Provide it with instructions, equip it with a `FileSearchToolDefinition`, and link the tool's resources to the ID of the vector store created in the previous step.

```csharp
PersistentAgent agentModel = await persistentAgentsClient.Administration.CreateAgentAsync(
            azure_foundry_model_id,
            name: "DotNetRAGAgent",
            tools: [new FileSearchToolDefinition()],
            instructions: """
                You are a helpful assistant that helps people find information in a set of files.  If you can't find the answer in the files, just say you don't know. Do not make up an answer.
                """,
            toolResources: new()
            {
                FileSearch = new()
                {
                    VectorStoreIds = { fileStore.Id },
                }
            });
```

**Step 5: Interact with the Agent**

Get the agent, start a new conversation thread, and ask a question. The agent will use the file search tool to find the answer within `demo.md`.

```csharp
AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentModel.Id);
AgentThread thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("What's graphrag?", thread));
```

**Output:**

```
GraphRAG is an AI-based content interpretation and search capability that utilizes large language models (LLMs) to parse data and create a knowledge graph. It enables the connection of information across large volumes of data, allowing it to answer complex questions that span multiple documents or that require thematic understanding. Its primary use is to support critical information discovery and analysis, especially for data that is noisy, fragmented, or involves misinformation. GraphRAG is intended for use by trained domain experts who can verify and interpret its outputs, making it suitable for specialized datasets and complex inquiries【4:0†demo.md】.
```

### Python Example

This example uses a Python script to perform the same actions.

**Step 1: Setup and Dependencies**

Import the necessary libraries for the Azure AI Agent Framework and handling environment variables.

```python
import os
from azure.ai.agents.models import FilePurpose,VectorStore,FileSearchTool
from azure.ai.projects.aio import AIProjectClient
from azure.identity.aio import AzureCliCredential
from dotenv import load_dotenv
from agent_framework import AgentRunResponse,ChatAgent,HostedFileSearchTool,HostedVectorStoreContent
from agent_framework.azure import AzureAIAgentClient

load_dotenv()
```

**Step 2: Define a Helper for Vector Store Creation**

This async function encapsulates the logic for uploading a file and creating a vector store from it.

```python
async def create_vector_store(client: AzureAIAgentClient) -> tuple[str, VectorStore]:
    """Create a vector store with sample documents."""
    file_path = '../files/demo.md'
    file = await client.project_client.agents.files.upload_and_poll(file_path=file_path, purpose=FilePurpose.AGENTS)
    print(f"Uploaded file, file ID: {file.id}")

    vector_store = await client.project_client.agents.vector_stores.create_and_poll(file_ids=[file.id], name="graph_knowledge_base")
    print(f"Created vector store, ID: {vector_store.id}")

    return file.id, vector_store
```

**Step 3: Create and Run the Agent**

The main execution block initializes the client, creates the vector store, defines the agent with the `FileSearchTool`, and runs a query. Note how the tool definitions and resources are passed during agent creation.

```python
async with (
        AzureCliCredential() as credential,
        AzureAIAgentClient(async_credential=credential) as chat_client,
    ):
        file_id, vector_store = await create_vector_store(chat_client)

        file_search = FileSearchTool(vector_store_ids=[vector_store.id])
        
        agent = chat_client.create_agent(
            name="PythonRAGDemo",
            instructions="""
                You are a helpful assistant that helps people find information in a set of files.  If you can't find the answer in the files, just say you don't know. Do not make up an answer.
                """,
            tools=file_search.definitions,  # Tools available to the agent
            tool_resources=file_search.resources,  # Resources for the tool
        )
                
        print("Agent created. You can now ask questions about the uploaded document.")

        query = "What is GraphRAG?"
        # The tool resources must be passed again at runtime
        response = await AgentRunResponse.from_agent_response_generator(agent.run_stream(query, tool_resources={"file_search": {"vector_store_ids": [vector_store.id]}}))
        print(f"Assistant: {response}")
```

**Output:**

```
Uploaded file, file ID: assistant-CiUQ5xjyFBC7TJyJ331FCF
Created vector store, ID: vs_BEbCqzLGHRRQsP0mMALlV5CV
Agent created. You can now ask questions about the uploaded document.
Assistant: GraphRAG is an AI-based content interpretation and search system that uses large language models to create a knowledge graph from a user-provided dataset. It connects information across large volumes of data to answer complex, thematic, or multi-document questions that are difficult to address through traditional keyword or vector search methods. The system is designed to support critical analysis and discovery, especially in contexts where information is noisy or spread across many sources. It emphasizes transparency, grounded responses, and resilience to injection attacks, although it relies on well-constructed indexing and human oversight for optimal performance【4:0†demo.md】.
```