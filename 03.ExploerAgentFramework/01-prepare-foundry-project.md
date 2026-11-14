# Studio 01: Prepare a Foundry project

This walkthrough creates a Microsoft Foundry project, deploys and tests a chat model, reviews the resource and project endpoints, and opens the project in Foundry Toolkit for Visual Studio Code. It prepares the resources used by the Foundry connection examples in this chapter; it does not create an agent.

Allow about 30–45 minutes. Foundry portal and Toolkit labels can change; if a label differs, use the linked Microsoft documentation for the current interface.

## Prerequisites, permissions, and cost

- An Azure subscription. Creating a Foundry resource/project and deploying a catalog model require elevated permissions (typically **Foundry Account Owner** or **Foundry Owner** at the applicable scope); using the project and deployed model requires **Foundry User** or an equivalent role at project scope. An administrator may need to assign roles and deployment quota. See [Foundry role-based access control](https://learn.microsoft.com/en-us/azure/foundry/concepts/rbac-foundry).
- Visual Studio Code and the **Foundry Toolkit** extension.
- Azure CLI, signed in to the same tenant and subscription used by Foundry (`az login`), and the .NET 10 SDK if you plan to run the C# sample.
- A model deployment quota in a supported region. Model and region availability can change; confirm availability in the catalog before creating resources.

Model inference is billable. Depending on the selected deployment type, a deployment can also incur charges while provisioned. Check [current Microsoft Foundry pricing](https://azure.microsoft.com/en-us/pricing/details/microsoft-foundry/) and deployment terms before proceeding. Costs depend on region, deployment type, and usage; deleting resources when finished prevents further deployment charges.

## Create a project and deploy a model

1. Open [Microsoft Foundry](https://ai.azure.com) and sign in with the Azure account for the subscription you intend to use.
2. Select **Create project** (or start building if this is your first visit). Give the project a unique name. In the advanced settings, choose the subscription, a new or existing resource group, and a region that supports the model you plan to deploy. Record the resource group and region.
3. Create the project and wait for its home page to load. A Foundry resource is created or selected to host the project.
4. Open **Discover > Models**, find `gpt-5.2`, and review its model card and regional availability. Select **Deploy** and use the default settings if they meet your quota and cost requirements. Record the deployment name shown after deployment. If `gpt-5.2` or its default deployment is unavailable in your subscription or region, choose another available chat model and use its deployment name below.
5. In the model playground, set the instructions to:

   ```text
   You are an AI assistant that can provide information and advice about AI software development.
   ```

6. Submit a prompt such as `Describe three key considerations for working with Large Language Models for AI application development.` Confirm that the playground returns a response.

## Find the endpoints

In the project home page, note the **project endpoint**, the **Azure OpenAI endpoint**, and (if displayed) the key. Under **Manage**, select the project's **Parent resource** to view the Foundry resource endpoint.

- **Foundry resource endpoint** identifies the parent resource and is used by resource-level services.
- **Project endpoint** identifies this project and is used by project APIs, including the Foundry Agent service. It commonly has a form like `https://<resource>.services.ai.azure.com/api/projects/<project>`.
- **Azure OpenAI endpoint** is a distinct endpoint for Azure OpenAI APIs and is not interchangeable with the project endpoint.
- A key supports key-based authentication. For local C# development, this repository's sample uses Microsoft Entra authentication through Azure CLI instead; do not put keys in source control.

## Open the project in Foundry Toolkit

1. In VS Code, install **Foundry Toolkit** from the Extensions view. Open its view and sign in to Azure. If the extension's current name or view differs, follow the [Foundry Toolkit setup guide](https://learn.microsoft.com/en-us/azure/foundry/how-to/develop/set-up-foundry-project-visual-studio-code).
2. In the Azure resources or Microsoft Foundry Resources view, choose the subscription and set the project you created as the default project.
3. Expand the project and **Models**, then select your deployment to inspect its details.
4. Open **Developer Tools > Build > Model playground**, select the deployment, and submit a prompt. Confirm that it returns a response in VS Code as it did in the portal.

## Configure the repository's C# sample

The [Foundry connection sample](./code_samples/dotNET/03-dotnet-agent-framework-msfoundry/Program.cs) reads `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`. Set the first to the **project endpoint** from the project home page, not the parent resource endpoint; set the second to the deployment name recorded above. The sample uses `AzureCliCredential`, so sign in with `az login` using an identity that can access the project and invoke the deployment. No API key is needed for this authentication path.

From the repository root, configure the environment and run the sample:

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project>"
export FOUNDRY_MODEL="<your-model-deployment-name>"
az login
cd 03.ExploerAgentFramework/code_samples/dotNET/03-dotnet-agent-framework-msfoundry
dotnet run
```

In PowerShell, set the variables with `$env:FOUNDRY_PROJECT_ENDPOINT="..."` and `$env:FOUNDRY_MODEL="..."`. The sample creates a Foundry agent version using the selected deployment and asks it for a haiku; running it sends a billable inference request. The project file currently has placeholder project references for locally built Agent Framework assemblies, so configure those references for your development environment before building if you use the build-from-source setup. The endpoint and model settings are still the required Foundry configuration.

## Expected outcomes and parity

The source exercise is [Prepare for an AI development project](https://github.com/hawarren/mslearn-ai-studio-hw/blob/b702dc2fd9f9411b5eb1f86b9259db6515cd9c48/Instructions/Exercises/01-Explore-ai-studio.md).

| Source exercise outcome | This walkthrough | Status |
| --- | --- | --- |
| Create a Foundry project | Create a project and record its region and resource group | Documented; live validation pending |
| Deploy and test a model | Deploy an available chat model and test a prompt in the portal | Documented; live validation pending |
| Inspect resource and project endpoints | Identify the parent resource, project endpoint, Azure OpenAI endpoint, and authentication option | Documented; live validation pending |
| Use Foundry Toolkit in VS Code | Set the default project, inspect the deployment, and test it in the model playground | Documented; live validation pending |
| Configure the C# project | Set `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`; authenticate with Azure CLI | Configuration documented; a runnable build depends on local Agent Framework project references |

**Verification limitation:** This walkthrough and its configuration were checked against the source exercise and the repository's existing C# sample. This environment has no Azure subscription, Foundry project, or VS Code session, so it could not demonstrate project creation, deployment, portal/Toolkit inference, or a live C# call. Those outcomes remain unverified; do not mark the exercise complete until they are demonstrated in an Azure environment.

## Clean up

Delete the model deployment if you are keeping the Foundry resource and no longer need the model. To remove everything created for this walkthrough, delete its resource group in the [Azure portal](https://portal.azure.com). Deleting a resource group permanently deletes every resource in it, so verify that it contains no resources you need before confirming.

For current project and extension steps, see the [Foundry project setup guide](https://learn.microsoft.com/en-us/azure/foundry/how-to/develop/set-up-foundry-project-visual-studio-code) and [Foundry resource quickstart](https://learn.microsoft.com/en-us/azure/foundry/tutorials/quickstart-create-foundry-resources).
