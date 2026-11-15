using System.Globalization;
using Microsoft.Extensions.AI;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
string data = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "data.txt"));
string summary = ExpenseClaim.Summarize(data);
Check(summary == "07-Mar-2025 - taxi: 24.00\n07-Mar-2025 - dinner: 65.50\n07-Mar-2025 - hotel: 125.90\nTotal: 215.40", "Unexpected summary");
Check(ExpenseClaim.Summarize("date,description,amount\r\nx,meal,0.10\r\ny,ride,0.20\r\n").EndsWith("Total: 0.30"), "Decimal total must be exact");
try
{
    ExpenseClaim.Summarize("date,description,amount\nx,meal,invalid");
    throw new Exception("Invalid amount was accepted");
}
catch (FormatException) { }
try
{
    ExpenseClaim.Summarize("date,description,amount\nx,meal,0.005");
    throw new Exception("Fractional cents were accepted");
}
catch (FormatException) { }

var tool = ExpenseClaim.CreateSubmissionTool();
Check(tool.Name == "submit_claim", "Unexpected tool name");
var original = Console.Out;
using var output = new StringWriter();
try
{
    Console.SetOut(output);
    object? result = await tool.InvokeAsync(new AIFunctionArguments
    {
        ["to"] = "expenses@contoso.com",
        ["subject"] = "Expense Claim",
        ["body"] = summary
    });
    Check(result?.ToString()?.Contains("no email was sent") == true, "Missing simulation result");
}
finally { Console.SetOut(original); }
Check(output.ToString().Contains("To: expenses@contoso.com\nSubject: Expense Claim\n" + summary), "Tool did not print claim");
Console.WriteLine("Expense claim checks passed.");
