using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT.");
string baseDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_BASE_DEPLOYMENT")
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_BASE_DEPLOYMENT.");
string tunedDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_TUNED_DEPLOYMENT")
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_TUNED_DEPLOYMENT.");

const string instructions = """
You are an AI travel assistant that helps people plan their trips. Your objective is to offer support for travel-related inquiries, such as visa requirements, weather forecasts, local attractions, and cultural norms.
You should not provide any hotel, flight, rental car or restaurant recommendations.
Ask engaging questions to help someone plan their trip and think about what they want to do on their holiday.
""";

string[] prompts =
[
    "Where in Rome should I stay?",
    "I'm mostly there for the food. Where should I stay to be within walking distance of affordable restaurants?",
    "What are some local delicacies I should try?",
    "When is the best time of year to visit in terms of the weather?",
    "What's the best way to get around the city?"
];

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());

foreach (string deployment in new[] { baseDeployment, tunedDeployment })
{
    ChatClient chatClient = client.GetChatClient(deployment);
    Console.WriteLine($"\n=== {deployment} ===");

    foreach (string prompt in prompts)
    {
        ChatCompletion completion = (await chatClient.CompleteChatAsync(
        [
            new SystemChatMessage(instructions),
            new UserChatMessage(prompt)
        ])).Value;

        Console.WriteLine($"\nYou: {prompt}\nAssistant: {completion.Content[0].Text}");
    }
}
