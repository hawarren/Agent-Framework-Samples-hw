using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;

public static class AstronomyTools
{
    private record Event(string Name, string Type, string Date, string[] Locations);

    private static readonly Event[] Events =
    [
        new("Quadrantids Meteor Shower", "meteor_shower", "01-03", ["north_america", "europe", "asia"]),
        new("Annular Solar Eclipse", "eclipse", "02-17", ["south_america", "africa", "antarctica"]),
        new("Total Lunar Eclipse", "eclipse", "03-03", ["north_america", "europe", "africa"]),
        new("Lyrids Meteor Shower", "meteor_shower", "04-22", ["north_america", "europe", "asia"]),
        new("Jupiter-Venus Conjunction", "conjunction", "05-01", ["north_america", "south_america", "europe", "asia", "africa", "australia"]),
        new("Saturn-Mars Conjunction", "conjunction", "07-10", ["north_america", "south_america", "europe", "asia", "africa", "australia"]),
        new("Partial Solar Eclipse", "eclipse", "08-12", ["europe", "africa"]),
        new("Perseids Meteor Shower", "meteor_shower", "08-12", ["north_america", "europe", "asia"]),
        new("Geminids Meteor Shower", "meteor_shower", "12-14", ["north_america", "south_america", "europe", "asia", "africa", "australia"])
    ];

    private static readonly Dictionary<string, decimal> Rates = new()
    {
        ["standard"] = 50, ["advanced"] = 120, ["premium"] = 300
    };

    private static readonly Dictionary<string, decimal> Multipliers = new()
    {
        ["low"] = 1, ["normal"] = 1.25m, ["high"] = 1.75m, ["urgent"] = 2.5m
    };

    public static IList<AITool> CreateTools() =>
    [
        AIFunctionFactory.Create((Func<string, string>)NextVisibleEvent, "next_visible_event"),
        AIFunctionFactory.Create(CalculateObservationCost, "calculate_observation_cost"),
        AIFunctionFactory.Create(GenerateObservationReport, "generate_observation_report")
    ];

    [Description("Find the next sample astronomical event visible from a continent, e.g. south_america.")]
    public static string NextVisibleEvent(
        [Description("Continent such as north_america, south_america, or australia.")] string location)
    {
        Console.WriteLine("[tool: next_visible_event]");
        return NextVisibleEvent(location, DateOnly.FromDateTime(DateTime.Now));
    }

    public static string NextVisibleEvent(string location, DateOnly today)
    {
        string normalized = location.Trim().ToLowerInvariant().Replace(' ', '_');
        string date = today.ToString("MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        Event? next = Events.FirstOrDefault(e =>
            string.CompareOrdinal(e.Date, date) >= 0 && e.Locations.Contains(normalized));
        return next is null
            ? JsonSerializer.Serialize(new { message = $"No upcoming events found for {location}." })
            : JsonSerializer.Serialize(new
            {
                @event = next.Name, type = next.Type, date = next.Date,
                visible_from = next.Locations.Order(StringComparer.Ordinal)
            });
    }

    [Description("Calculate telescope rental cost in dollars using the tier, hours, and priority.")]
    public static string CalculateObservationCost(
        [Description("Telescope tier: standard, advanced, or premium.")] string telescope_tier,
        [Description("Number of hours, greater than zero.")] decimal hours,
        [Description("Priority: low, normal, high, or urgent.")] string priority)
    {
        Console.WriteLine("[tool: calculate_observation_cost]");
        string tier = telescope_tier.Trim().ToLowerInvariant();
        string pri = priority.Trim().ToLowerInvariant();
        if (!Rates.TryGetValue(tier, out decimal rate))
            return JsonSerializer.Serialize(new { error = $"Unknown telescope tier '{telescope_tier}'. Choose standard, advanced, or premium." });
        if (!Multipliers.TryGetValue(pri, out decimal multiplier))
            return JsonSerializer.Serialize(new { error = $"Unknown priority '{priority}'. Choose low, normal, high, or urgent." });
        if (hours <= 0)
            return JsonSerializer.Serialize(new { error = "Hours must be greater than zero." });

        decimal baseCost = rate * hours;
        return JsonSerializer.Serialize(new
        {
            telescope_tier = tier, hours, hourly_rate = rate, priority = pri,
            priority_multiplier = multiplier, base_cost = baseCost, total_cost = baseCost * multiplier
        });
    }

    [Description("Generate a local observation report and return its filename.")]
    public static string GenerateObservationReport(
        [Description("Name of the event being observed.")] string event_name,
        [Description("Observer's continent.")] string location,
        [Description("Telescope tier: standard, advanced, or premium.")] string telescope_tier,
        [Description("Number of hours, greater than zero.")] decimal hours,
        [Description("Priority: low, normal, high, or urgent.")] string priority,
        [Description("Name of the observer or organization.")] string observer_name)
    {
        Console.WriteLine("[tool: generate_observation_report]");
        string cost = CalculateObservationCost(telescope_tier, hours, priority);
        using JsonDocument costDocument = JsonDocument.Parse(cost);
        if (costDocument.RootElement.TryGetProperty("error", out _))
            return cost;

        string next = NextVisibleEvent(location);
        string filename = $"report_{Guid.NewGuid():N}.txt";
        File.WriteAllText(filename, $"""
            CONTOSO OBSERVATORIES - SESSION REPORT
            Date: {DateTime.Now:yyyy-MM-dd HH:mm}
            Observer: {observer_name}
            Event: {event_name}
            Location: {location}
            Next visible event: {next}
            Telescope booking and cost: {cost}
            """);
        return JsonSerializer.Serialize(new { status = "Report generated", file = filename });
    }
}
