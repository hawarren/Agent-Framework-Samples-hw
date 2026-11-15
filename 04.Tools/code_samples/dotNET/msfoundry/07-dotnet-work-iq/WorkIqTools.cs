using ModelContextProtocol.Client;

public static class WorkIqTools
{
    public static IReadOnlyList<McpClientTool> Select(IEnumerable<McpClientTool> tools)
    {
        var selected = tools.Where(t => t.Name == "ask").ToList();
        return selected.Count == 1
            ? selected
            : throw new InvalidOperationException("Work IQ did not expose exactly one ask tool. Check the installed version and consent.");
    }
}

public static class WorkplacePrompts
{
    public static string Create(string choice, string input) => choice switch
    {
        "1" => $"Prepare me for this meeting: {input}. Find time, attendees and agenda, relevant recent emails and previous meetings, key decisions and suggested discussion points.",
        "2" => $"Give me the status of project {input}. Search emails, Teams and meetings for updates, decisions, changes, blockers, next steps and deadlines.",
        "3" => $"Find my open action items for {(string.IsNullOrWhiteSpace(input) ? "this week" : input)} in meeting notes, emails and Teams mentions. Include deadlines and prioritize where possible.",
        "4" => input,
        _ => throw new ArgumentOutOfRangeException(nameof(choice))
    };
}
