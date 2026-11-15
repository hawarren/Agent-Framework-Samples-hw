# Studio 02: Compare models and run a synthetic-data evaluation

This exercise follows the [Explore and compare models](https://github.com/hawarren/mslearn-ai-studio-hw/blob/b702dc2fd9f9411b5eb1f86b9259db6515cd9c48/Instructions/Exercises/02-model-catalog-evaluation.md) walkthrough. It uses the Microsoft Foundry portal to compare catalog benchmarks, test deployments side by side, and evaluate a model against a synthetically generated dataset. This is a separate activity from the [DevUI and tracing examples](./README.md), which focus on observing and debugging agent runs.

**Time:** approximately 45 minutes.

## Prerequisites, setup, and cost

- An Azure subscription with permission to create a Microsoft Foundry project and deploy models. The selected subscription and region must have access and quota for the models used in the exercise.
- A browser and access to [Microsoft Foundry](https://ai.azure.com). No local code or framework installation is needed.
- Allow for model deployments, synthetic-data generation, evaluation, and analysis to take several minutes.
- Model inference and evaluation (including synthetic-data generation and judge-model calls) are billed according to the deployed models, region, and usage. Check current Azure pricing and quota before starting; no fixed cost is assumed here. Delete deployments and resources when finished.

In Foundry, enable **New Foundry** if prompted and create a project with a unique name. Use the default Foundry resource name, select your subscription, create or select a resource group, and choose a currently recommended region that supports the target models. Wait for project creation before continuing.

> Portal labels, model availability, and deployment options can change. If a named model is unavailable in your subscription or region, select a comparable model only if you can still follow the deployment and evaluation steps; record the substitution when comparing results.

## Explore the catalog and benchmarks

1. In the project, open **Discover > Models**.
2. Search for **gpt-5.2**, select it, and review its model card, especially **Details** and **Benchmarks**. Benchmarks are comparative signals, not guarantees of performance for your workload.
3. Return to the catalog and select **View leaderboard**. Review the quality, safety, and performance rankings and the trade-off chart.
4. Compare **gpt-5.2** and **gpt-5-mini** using **Benchmark Cost**, then **Throughput**, then **Safety**. Add other models if useful. In the comparison table, select both models and choose **Compare models**.
5. Review the side-by-side performance benchmarks, input and output formats, context limits, endpoints, and supported features. Note the trade-offs relevant to your scenario rather than treating one benchmark as a universal winner.

## Deploy and test in the playground

1. From the catalog, select **gpt-5.2**, choose **Deploy**, and keep the default settings. Record the deployment name.
2. In the playground's **Model** list, choose **Browse more models**, find **gpt-5-mini**, and deploy it using the available default settings. Record its deployment name.
3. With **gpt-5-mini** selected, add the **gpt-5.2** deployment in **Compare models**. In each chat pane, submit:

   ```text
   I have a fox, a chicken, and a bag of grain that I need to take over a river in a boat. I can only take one thing at a time. If I leave the chicken and the grain unattended, the chicken will eat the grain. If I leave the fox and the chicken unattended, the fox will eat the chicken. How can I get all three things across the river without anything being eaten?
   ```

4. Follow up with `Explain your reasoning.` Compare whether each answer is valid, how clearly it explains the crossings, and its response style. Record observations; responses and model behavior can vary between runs.

## Create and run a synthetic-data evaluation

The evaluation measures model responses against a generated dataset and selected evaluators. It is not a substitute for testing representative, reviewed data from your application.

### 1. Select the target

1. In the playground, open **Evaluations** and select **Create**.
2. Choose **Model** as the evaluation target. Clear any preselected deployments and select only **gpt-5.2**, then choose **Next**.

### 2. Generate the dataset

1. Under **Dataset source**, select **Synthetic generation**, then **Generate**.
2. Keep the default dataset name and set:
   - **Model:** gpt-5.2
   - **Number of rows:** 45
   - **Prompt:** `Create various travel related questions, and include some content safety and security tests`
   - **Seed data:** leave blank
3. Choose **Next**. When you submit the evaluation, Foundry uses a model deployment to generate the evaluation inputs; generation consumes model usage and adds time to the run.

### 3. Configure the target model

Set the **Developer** prompt to:

```text
You are a helpful travel assistant that provides accurate, detailed, and practical travel advice to help users plan their trips.
```

Leave other settings at their defaults and choose **Next**.

### 4. Choose criteria and submit

Review the suggested evaluators. Remove all criteria under **Agents** and **Safety**, leaving the other suggested evaluators enabled, then choose **Next**. On **Review**, verify the target, dataset, and criteria; name the evaluation (for example, `travel-assistant-eval`) and select **Submit**. Wait for the run to complete.

### 5. Review results

1. Open the completed evaluation run and review the overview metrics and per-row results. Inspect additional result columns and any failures; scores depend on generated data, model responses, and evaluator behavior.
2. Select **Analyze results**, choose **gpt-5.2**, and select **Start analysis**.
3. Review the failure clusters, example responses, and suggested improvements. Decide whether each failure indicates a real issue for your use case. Do not treat generated examples or AI suggestions as ground truth.

## Expected outcomes and parity

| Learning outcome | Demonstration | Status |
| --- | --- | --- |
| Browse catalog model cards and compare benchmark/cost/throughput/safety trade-offs | Catalog, leaderboard, trade-off chart, and comparison table above | Walkthrough documented |
| Test two model deployments with the same prompt and follow-up | Side-by-side playground steps above | Walkthrough documented |
| Generate a synthetic dataset, configure a Foundry evaluation, and run it | Evaluation steps 1–4 above | Walkthrough documented |
| Review metrics, individual results, failures, and analysis suggestions | Results steps above | Walkthrough documented |

The portal procedure is documented from the linked exercise, but it has **not been run against a live Azure subscription in this repository environment**. Therefore service behavior, current model availability, exact UI labels, quotas, and resulting scores remain unverified here; this walkthrough does not claim live validation or fixed expected scores. Access to the required deployments and evaluation features is a prerequisite.

## Cleanup

When finished, delete the project or the resource group created for this exercise. In the Azure portal, review the resource group's contents before choosing **Delete resource group**, enter its name, and confirm. Do not delete a shared resource group. Verify that the model deployments and project resources are removed to prevent further charges.
