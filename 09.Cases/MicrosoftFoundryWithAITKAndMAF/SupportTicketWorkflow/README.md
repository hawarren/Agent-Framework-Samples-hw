# ContosoPay support-ticket workflow (Agents 06)

This is the portal-based [Build a workflow in Microsoft Foundry](https://github.com/hawarren/mslearn-ai-agents-hw/blob/5f58c207928d0dd3963f06066b16bc6c6b0d3ce4/Instructions/Exercises/06-build-workflow-ms-foundry.md) exercise with a C# client. The workflow runs in Foundry, not in the local [recruitment YAML runner](../CreateWorkflowWithYAML/).

## Prerequisites and cost

You need an Azure subscription, access to the new [Foundry portal](https://ai.azure.com), permission to create a project, model deployments, agents and workflows and to invoke them, a supported model deployment, .NET 10 SDK and Azure CLI. Sign in with `az login` using an identity with project access. The workflow builder and the .NET workflow APIs are preview features; availability, permissions, model choices and portal labels can change. Model inference and Azure resources can incur charges, including each preview and client run; consult [Foundry pricing](https://azure.microsoft.com/pricing/details/ai-foundry/) and use a budget.

## Build the workflow in Foundry

1. In the new Foundry portal create/select a project. Under **Build > Agents > Workflows**, choose **Create > Blank workflow**; save it as `ContosoPay-Customer-Support-Triage`.
2. Add **Data transformation > Set variable**, name it `SupportTickets` (`Local.SupportTickets`), and set its value to:

   ```json
   [
     "The API returns a 403 error when creating invoices, but our API key hasn't changed.",
     "Is there a way to export all invoices as a CSV?",
     "I was charged twice for the same invoice last Friday and my customer is also seeing two receipts. Can someone fix this?"
   ]
   ```

3. Add **For each** over `Local.SupportTickets`, with loop value variable `CurrentTicket`. All following nodes belong inside this loop.
4. Add **Invoke > Agent**, create `Triage-Agent` using a supported deployed model, and set **Parameters > Text format > JSON Schema** to:

   ```json
   {
     "name": "category_response",
     "schema": {
       "type": "object",
       "properties": {
         "customer_issue": { "type": "string" },
         "category": { "type": "string" },
         "confidence": { "type": "number" }
       },
       "additionalProperties": false,
       "required": ["customer_issue", "category", "confidence"]
     },
     "strict": true
   }
   ```

   Set its instructions to:

   ```text
   Classify the user's problem description into exactly ONE category from the list below. Provide a confidence score from 0 to 1.

   Billing
   - Charges, refunds, duplicate payments
   - Missing or incorrect payouts
   - Subscription pricing or invoices being charged

   Technical
   - API errors, integrations, webhooks
   - Platform bugs or unexpected behavior

   General
   - How-to questions
   - Feature availability
   - Data exports, reports, or UI navigation

   Important rules
   - Questions about exporting, viewing, or downloading invoices are General, not Billing
   - Billing ONLY applies when money was charged, refunded, or paid incorrectly
   ```

   In **Node settings**, set input to `Local.CurrentTicket`, save the output message as `TriageOutputText`, and save the output JSON object as `TriageOutputJson`.
5. Add **Flow > If/Else** after triage. The first path's condition is:

   ```text
   Local.TriageOutputJson.confidence > 0.6
   ```

   In **Else**, add **Basics > Deliver a message**:

   ```text
   The support ticket classification has low confidence. Requesting more details about the issue: "{Local.CurrentTicket}"
   ```

6. Under the confidence **If**, add another **If/Else** with:

   ```text
   Local.TriageOutputJson.category = "Billing"
   ```

   Under its **If**, add **Deliver a message** with `Escalate billing issue to human support team.` Under its **Else**, add **Invoke > Agent**, create `Resolution-Agent` with a supported model, and set instructions to:

   ```text
   You are a customer support resolution assistant for ContosoPay, a B2B payments and invoicing platform.

   Your task is to draft a clear, professional, and friendly support response based on the issue category and customer message.

   Guidelines:
   If the issue category is Technical:
   Suggest 1–2 common troubleshooting steps at a high level.
   Avoid asking for logs, credentials, or sensitive data.
   Do not imply fault by the customer.
   If the issue category is General:
   Provide a concise, helpful explanation or guidance.
   Keep the response under 5 sentences.

   Tone:
   Professional, calm, and supportive
   Clear and concise
   No emojis

   Output:
   Return only the drafted response text.
   Do not include internal reasoning or analysis.
   ```

   Set its input to `Local.TriageOutputText` and save its output message as `ResolutionOutputText`.
7. Save, **Preview**, and send `Start processing support tickets.` Inspect the run and its messages for all three iterations. Expected, not guaranteed: the API 403 is Technical and receives troubleshooting; invoice export is General and receives guidance; the duplicate charge is Billing and emits the escalation message. Any classification with confidence **at or below 0.6** instead requests details, even for Billing. Agent output and confidence vary between runs.
8. Save the workflow version and make it callable from the project (publish/deploy it if the portal requires that step). In the workflow's **Code > .env variables**, copy the project endpoint. Record the workflow name and, if selecting a specific version, its version.

## Run the deployed workflow from C#

From this directory:

```bash
dotnet build
export FOUNDRY_PROJECT_ENDPOINT="https://YOUR-RESOURCE.services.ai.azure.com/api/projects/YOUR-PROJECT"
export FOUNDRY_WORKFLOW_NAME="ContosoPay-Customer-Support-Triage"
# Optional: otherwise the service selects the default version.
export FOUNDRY_WORKFLOW_VERSION="YOUR-WORKFLOW-VERSION"
az login
dotnet run --no-build
```

The client creates a conversation, invokes the named workflow through the Foundry project Responses API, prints streamed workflow action statuses, retrieves and prints the completed response text, then deletes the conversation even on failure. The prompt triggers the workflow's **fixed portal array**; it does not supply tickets. If the run doesn't complete, the program fails rather than reporting success. This is a live, opt-in check and incurs model charges.

## Deterministic routing checks

Run `python3 -m unittest -v test_routing.py` from this directory. The offline checks read the two expressions above and evaluate a small model of their branches against fixed triage results:

| Category | Confidence | Expected path |
|---|---:|---|
| Billing | 0.59 | request details |
| Billing | 0.60 | request details |
| Billing | 0.61 | escalation message |
| Technical | 0.61 | resolution agent |
| General | 0.90 | resolution agent |

These are contract checks, **not** tests of the deployed portal or model. To verify actual portal routing deterministically, use a disposable workflow copy. For each row, temporarily replace the confidence condition with the literal comparison (for example `0.60 > 0.6`); for high-confidence rows also replace the category condition with the corresponding literal comparison (for example `"Billing" = "Billing"` or `"Technical" = "Billing"`). Preview and inspect the executed branch for each case. Restore both original expressions before saving the version used by the client. Never expect a prompt to reliably force an exact model confidence.

## Parity and limitations

| Exercise outcome | This sample | Status |
|---|---|---|
| Portal ticket array and loop; triage into three categories | Steps 2–4 | Instructions provided; live demonstration pending |
| Confidence handling and category routing | Steps 5–7; offline boundary checks | Contract tested offline; portal demonstration pending |
| Billing escalation and non-billing response | Step 6 | Escalation is only a message, not a human-ticket integration; live demonstration pending |
| Invoke workflow and consume streamed results | C# client above | Buildable; live invocation pending |

The exercise's schema requires the three fields but does not enforce category values or confidence range; a high-confidence category other than exactly `Billing` takes the resolution branch. The client uses preview SDKs; their workflow event and response behavior may change. The published .NET SDK's recorded workflow streaming test covers a legacy workflow, not this exact portal workflow. No Foundry credentials or deployment were available here, so portal behavior, actual output visibility, version selection and the live client are **not verified**. Do not mark parity complete until the preview, deterministic portal checks and client run have all been observed.

## Cleanup

The client deletes its conversation automatically. Delete the workflow and the two agents/model deployments when no longer needed. If the project and its resource group were created solely for this lab, delete the resource group in the [Azure portal](https://portal.azure.com) after checking it contains nothing you need; resource-group deletion removes all its resources. Review remaining resources and costs separately.
