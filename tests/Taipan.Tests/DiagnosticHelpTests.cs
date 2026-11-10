using System.Text.RegularExpressions;

namespace Taipan.Tests;

/// <summary>
/// Every error shown in a diagnostic explanation says what to write instead (M57): each diagnostic
/// line in an <c>error</c> block of <c>website/diagnostics/&lt;code&gt;.md</c> is followed by a
/// <c>help:</c> line, or the explanation has a <c>&lt;!-- no help: &lt;why&gt; --&gt;</c> note. The
/// explanations show the cases a newcomer hits, so this is where a missing help line costs most.
/// </summary>
public class DiagnosticHelpTests
{
    private static readonly string Explanations = Path.Combine(RepoPaths.Root, "website", "diagnostics");

    private static readonly Regex ErrorBlock = new(@"```error\n(?<text>.*?)\n```", RegexOptions.Singleline);
    private static readonly Regex DiagnosticLine = new(@"^\S+:\d+:\d+: (error|warning) (?<code>TP\w+): (?<message>.*)$");
    private static readonly Regex NoHelp = new(@"<!-- no help: .+? -->");

    public static TheoryData<string> Explained() =>
        [.. Directory.EnumerateFiles(Explanations, "*.md").Select(f => Path.GetFileName(f)).Order(StringComparer.Ordinal)];

    [Theory]
    [MemberData(nameof(Explained))]
    public void EveryShownErrorHasHelp(string file)
    {
        var text = File.ReadAllText(Path.Combine(Explanations, file)).Replace("\r\n", "\n");
        if (NoHelp.IsMatch(text))
            return;
        var missing = new List<string>();
        foreach (Match block in ErrorBlock.Matches(text))
        {
            var lines = block.Groups["text"].Value.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (DiagnosticLine.Match(lines[i]) is { Success: true } m && (i + 1 >= lines.Length || !lines[i + 1].StartsWith("  help:")))
                    missing.Add($"{m.Groups["code"].Value}: {m.Groups["message"].Value}");
            }
        }
        Assert.True(missing.Count == 0, $"{file} shows errors with no help line: {string.Join("; ", missing)}");
    }
}
