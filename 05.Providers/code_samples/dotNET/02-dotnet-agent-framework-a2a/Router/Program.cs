using A2A;

string titleUrl = Environment.GetEnvironmentVariable("TITLE_AGENT_URL") ?? "http://localhost:5001";
string outlineUrl = Environment.GetEnvironmentVariable("OUTLINE_AGENT_URL") ?? "http://localhost:5002";
string topic = args.Length > 0
    ? string.Join(' ', args)
    : "React programming";

var titleResolver = new A2ACardResolver(new Uri(titleUrl));
var outlineResolver = new A2ACardResolver(new Uri(outlineUrl));
var titleCard = await titleResolver.GetAgentCardAsync();
var outlineCard = await outlineResolver.GetAgentCardAsync();

Console.WriteLine($"Discovered: {titleCard.Name} and {outlineCard.Name}");
var router = new ArticleRouter(
    new A2ARemoteTextAgent(await titleResolver.GetAIAgentAsync()),
    new A2ARemoteTextAgent(await outlineResolver.GetAIAgentAsync()));
ArticleDraft draft = await router.CreateAsync(topic);

Console.WriteLine($"\nTitle: {draft.Title}\n");
Console.WriteLine($"Outline:\n{draft.Outline}");
