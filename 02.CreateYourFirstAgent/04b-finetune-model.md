# Fine-tune and compare a travel model

In this portal-first exercise, you'll fine-tune a model on travel conversations, then compare it with its base model using the same instructions and prompts. The examples teach a playful, engaging voice while avoiding unsupported hotel, flight, rental-car, and restaurant recommendations.

Allow about 90 minutes. Fine-tuning and deployment can take an hour or more, and timing varies with regional capacity. You can continue the base-model comparison while the job runs.

## Prerequisites and costs

- An Azure subscription with permission to create a Microsoft Foundry project, deploy models, and start fine-tuning jobs. Check your quota and access before starting.
- A region and base model that currently support supervised fine-tuning for the selected model. The source exercise used `gpt-5` in North Central US or Sweden Central; availability changes, so verify the current [fine-tuning model availability](https://learn.microsoft.com/azure/foundry/foundry-models/concepts/models-sold-directly-by-azure#fine-tuning-models) and portal options before choosing a region.
- A browser signed in to [Microsoft Foundry](https://ai.azure.com). The optional C# example requires the .NET 10 SDK, Azure CLI, and permission to invoke both deployments.

Training, storage, and model deployments can incur charges. A running deployment can incur hosting charges even while idle. Review current regional pricing, deployment-type costs, quota, and estimates in Azure before submitting a job. Delete deployments and resources when finished; this guide does not promise a fixed price.

## Create the project and baseline deployment

1. In Foundry, create or select a project and resource in a region that supports fine-tuning for the model you intend to use. Keep the subscription, resource group, and project details available for cleanup.
2. From **Discover > Models**, find `gpt-5` (or a currently supported equivalent), review its model card, and deploy it using the portal's available defaults. Record the deployment name; this is the base-model deployment used in the comparison.
3. Open its playground and ask `What can you do?` once without custom instructions. This baseline helps distinguish model defaults from the behavior prompted or learned below.

## Review the training data

Download [`travel-finetune-hotel.jsonl`](https://microsoftlearning.github.io/mslearn-ai-studio/data/travel-finetune-hotel.jsonl) and open it as text. It is JSON Lines: one JSON object per line. Each example contains a `messages` array with a shared system message, a travel-related user request, and the desired assistant answer. For example, an example asks for a must-see in Paris and answers in an enthusiastic voice with attractions and a follow-up question.

Before uploading, verify that:

- Every line is valid JSON and contains the expected roles and non-empty text.
- The examples consistently teach the intended tone and boundaries. In particular, they should not train the assistant to promise bookings or give hotel, flight, rental-car, or restaurant recommendations.
- The file is the expected dataset, has no accidental `.txt` suffix, and contains no private or sensitive data.

The dataset is a teaching example, not a guarantee of accuracy or broad travel coverage. Fine-tuning teaches response patterns; it does not add current travel facts.

## Configure and monitor supervised fine-tuning

1. In the project, open **Fine-tune** and choose **Fine-tune**.
2. Select the same base model deployed above, `gpt-5` if available.
3. Use **Supervised** customization and **Standard** training. Upload the reviewed JSONL file and set the suffix to `ft-travel`.
4. Select automatic deployment on job completion and the **Developer** deployment type if that option is available for your region/model. Leave other hyperparameters at their defaults for this comparison.
5. Review the selected model, dataset, deployment choice, and cost implications, then submit.
6. Return to **Fine-tune**, select the job, and use **Monitor** to check progress and **Logs** (if shown) to review task events. Job submission is not completion: wait for a successful terminal status before testing.
7. Verify the fine-tuned deployment under **Deployments**. If automatic deployment failed, open the completed job and deploy its resulting model manually. Record the actual deployment name for the C# example.

Portal names and available controls may change. If an option listed above is absent, use the current model/job details and supported deployment types shown in Foundry rather than substituting an unsupported model or region.

## Compare the base and fine-tuned models

For a fair comparison, use the same instructions in each deployment's playground:

```text
You are an AI travel assistant that helps people plan their trips. Your objective is to offer support for travel-related inquiries, such as visa requirements, weather forecasts, local attractions, and cultural norms.
You should not provide any hotel, flight, rental car or restaurant recommendations.
Ask engaging questions to help someone plan their trip and think about what they want to do on their holiday.
```

Ask each deployment the same prompts in separate chats:

1. `Where in Rome should I stay?`
2. `I'm mostly there for the food. Where should I stay to be within walking distance of affordable restaurants?`
3. `What are some local delicacies I should try?`
4. `When is the best time of year to visit in terms of the weather?`
5. `What's the best way to get around the city?`

Compare whether each model stays within the stated boundaries, uses the training set's friendly and engaging style, asks useful follow-up questions, and handles factual questions responsibly. Record observations rather than expecting identical answers; model output is nondeterministic. Fine-tuning does not guarantee improvement, and the prompt itself may account for much of the behavior.

### Optional C# comparison

This small client sends identical system instructions and prompts to both deployments. It does not create, train, or deploy models; complete the portal exercise first. Set `AZURE_OPENAI_ENDPOINT` to the Azure OpenAI endpoint for the Foundry resource and use the exact deployment names recorded above. The signed-in Azure CLI identity needs permission to invoke the deployments.

```bash
az login
export AZURE_OPENAI_ENDPOINT="https://<resource>.openai.azure.com/"
export AZURE_OPENAI_BASE_DEPLOYMENT="gpt-5"
export AZURE_OPENAI_TUNED_DEPLOYMENT="<fine-tuned deployment name>"
cd 02.CreateYourFirstAgent/code_samples/dotNET/foundry-finetune-travel
dotnet run
```

In PowerShell, set the same values with `$env:AZURE_OPENAI_ENDPOINT="..."` and corresponding `$env:AZURE_OPENAI_BASE_DEPLOYMENT` and `$env:AZURE_OPENAI_TUNED_DEPLOYMENT` assignments.

The program uses `AzureCliCredential`; use an appropriate managed identity or other `DefaultAzureCredential` source when adapting it for an application. Requests are billable.

## Expected outcomes and parity

| Exercise outcome | Demonstration |
| --- | --- |
| Review the travel fine-tuning data | Inspect the JSONL records, schema, style, and recommendation boundaries |
| Configure supervised fine-tuning | Submit a Standard job with the selected base model and reviewed dataset |
| Monitor the job and deploy the result | Check job status and logs; verify or manually create the tuned deployment |
| Compare base and tuned travel behavior | Run the same instruction and five prompts against both deployments in Foundry; optionally use the C# client |
| Understand operational costs and cleanup | Review deployment charges and delete the test deployments/project or resource group |

This documents the portal workflow and provides a buildable C# invocation sample; it does not claim that a live Azure training run has been completed. Service access, model/region availability, quota, job completion, and observed quality must be verified in your own subscription. Treat a result as demonstrated only after you have a successful job, a working tuned deployment, and recorded a side-by-side comparison.

## Cleanup

When finished, delete both the base and fine-tuned deployments to stop deployment charges. Then delete the project or its resource group in the Azure portal if it was created only for this exercise. Confirm that shared resources are not used by other projects before deleting the resource group.
