using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.AI;

public static class ExpenseClaim
{
    public static string Summarize(string data)
    {
        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 2 || lines[0] != "date,description,amount")
            throw new FormatException("Expected a date,description,amount header and at least one expense.");

        decimal total = 0;
        var items = new List<string>();
        foreach (string line in lines.Skip(1))
        {
            string[] fields = line.Split(',', StringSplitOptions.TrimEntries);
            if (fields.Length != 3 || fields.Any(string.IsNullOrWhiteSpace)
                || !decimal.TryParse(fields[2], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out decimal amount))
                throw new FormatException($"Invalid expense row: {line}");
            total += amount;
            items.Add($"{fields[0]} - {fields[1]}: {amount.ToString("0.00", CultureInfo.InvariantCulture)}");
        }

        return $"{string.Join("\n", items)}\nTotal: {total.ToString("0.00", CultureInfo.InvariantCulture)}";
    }

    public static AIFunction CreateSubmissionTool() =>
        AIFunctionFactory.Create(SubmitClaim, "submit_claim");

    [Description("Simulate submitting an expense claim by printing an email; no email is sent.")]
    public static string SubmitClaim(
        [Description("Email recipient")] string to,
        [Description("Email subject")] string subject,
        [Description("Itemized expenses and total")] string body)
    {
        Console.WriteLine($"\nSimulated email\nTo: {to}\nSubject: {subject}\n{body}\n");
        return "Claim submission simulated; no email was sent.";
    }
}
