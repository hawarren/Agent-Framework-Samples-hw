using Azure.AI.Projects;
using Azure.Identity;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to your Foundry project endpoint.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1";
var client = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());

var workflow = FeedbackPipeline.BuildFoundry(client, model);

string feedback = args.Length > 0 ? string.Join(" ", args) : FeedbackPipeline.DefaultFeedback;
var messages = await FeedbackPipeline.RunAsync(workflow, feedback);
FeedbackPipeline.Display(messages, Console.Out);
