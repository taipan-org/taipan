using System.Text.RegularExpressions;

namespace Taipan.Tests;

/// <summary>
/// Every diagnostic code has an explanation (M56): <c>website/diagnostics/&lt;code&gt;.md</c>, which
/// the website's generator puts on the code's page. It has a "What it means" and a "How to fix it"
/// section, and, for a compiler code, a wrong example whose error block shows the code and a fixed
/// example with an output block and no error block. Both run as website samples, so the fixed one
/// compiles without even a warning. Codes that <c>taipan run</c> never reports need only the
/// sections: the SDK's TPN codes, from <c>dotnet build</c> on a project, and the ones only a native
/// build, a library build or <c>taipan test</c> reports.
/// </summary>
public class DiagnosticExplanationTests
{
    private static readonly string Explanations = Path.Combine(RepoPaths.Root, "website", "diagnostics");

    // Reported only by a native build (TP0233), a library build (TP0302) or `taipan test` (TP0303).
    private static readonly HashSet<string> NotFromRun = ["TP0233", "TP0302", "TP0303"];

    // A taipan block and the output or error block straight after it.
    private static readonly Regex Pair = new(
        @"```taipan[^\n]*\n.*?\n```\s*\n```(?<kind>output|error)\n(?<text>.*?)\n```", RegexOptions.Singleline);

    public static TheoryData<string> Codes()
    {
        var spec = File.ReadAllText(Path.Combine(RepoPaths.Root, "SPEC.md")).Replace("\r\n", "\n");
        var section = spec[spec.IndexOf("\n## 9. Diagnostics")..spec.IndexOf("\n## 10. ")];
        var rows = section[section.IndexOf("| Code | Meaning |")..].Split('\n').Skip(2).TakeWhile(l => l.StartsWith("| TP"));
        return [.. rows.Select(r => r.Split('|')[1].Trim()).Distinct()];
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public void EveryCodeIsExplained(string code)
    {
        var path = Path.Combine(Explanations, $"{code.ToLowerInvariant()}.md");
        Assert.True(File.Exists(path), $"no explanation {path}");
        var text = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.Contains("## What it means", text);
        Assert.Contains("## How to fix it", text);
        if (code.StartsWith("TPN") || NotFromRun.Contains(code))
            return;
        var pairs = Pair.Matches(text);
        Assert.True(pairs.Any(p => p.Groups["kind"].Value == "error" && Regex.IsMatch(p.Groups["text"].Value, $@"(error|warning) {code}:")),
            $"{code}'s explanation has no example whose error block shows {code}");
        Assert.True(pairs.Any(p => p.Groups["kind"].Value == "output"),
            $"{code}'s explanation has no fixed example with an output block");
    }
}
