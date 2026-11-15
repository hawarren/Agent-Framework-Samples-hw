using Azure.Core;
using Azure.Identity;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_OPENAI_ENDPOINT to the OpenAI v1 endpoint.");
string model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL to the deployed model name.");
string prompt = Environment.GetEnvironmentVariable("FOUNDRY_TEST_PROMPT")
    ?? "What should I do if I cut myself?";

var credential = new DefaultAzureCredential();
var token = await credential.GetTokenAsync(
    new TokenRequestContext(["https://ai.azure.com/.default"]));

using var httpClient = new HttpClient();
using var request = new HttpRequestMessage(
    HttpMethod.Post,
    $"{endpoint.TrimEnd('/')}/chat/completions");
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
request.Content = JsonContent.Create(new
{
    model,
    messages = new[] { new { role = "user", content = prompt } }
});

using var response = await httpClient.SendAsync(request);
string responseBody = await response.Content.ReadAsStringAsync();

if (response.StatusCode == HttpStatusCode.BadRequest && IsPromptFiltered(responseBody))
{
    Console.WriteLine("The prompt was blocked by the content filter.");
    return;
}

response.EnsureSuccessStatusCode();

using var result = JsonDocument.Parse(responseBody);
foreach (JsonElement choice in result.RootElement.GetProperty("choices").EnumerateArray())
{
    string? finishReason = choice.GetProperty("finish_reason").GetString();
    if (finishReason == "content_filter")
    {
        Console.WriteLine("The completion was blocked by the content filter.");
        continue;
    }

    JsonElement content = choice.GetProperty("message").GetProperty("content");
    if (content.ValueKind == JsonValueKind.String)
    {
        Console.WriteLine(content.GetString());
    }
}

static bool IsPromptFiltered(string responseBody)
{
    try
    {
        using JsonDocument error = JsonDocument.Parse(responseBody);
        return error.RootElement.TryGetProperty("error", out JsonElement details)
            && details.TryGetProperty("code", out JsonElement code)
            && code.GetString() == "content_filter";
    }
    catch (JsonException)
    {
        return false;
    }
}
