using System.Text.Json.Nodes;

namespace Taipan.Tests;

/// <summary>
/// M58 (SPEC §16.3–§16.5): hover, completion and signature help show documentation: docstrings,
/// the built-ins' descriptions, and .NET members' summaries.
/// </summary>
public sealed class EditorDocumentationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"taipan-docs-{Guid.NewGuid():N}");

    private const string Helper = "\"\"\"Helps out.\"\"\"\n\ndef aid() -> int:\n    return 1\n";

    private const string Main = """"
        import sys
        import helper
        from dotnet.System.Text import StringBuilder

        def greet(name: str) -> str:
            """Says hello to name.

            The name is used as given."""
            return "hello, " + name

        class Counter:
            """Counts up."""
            n: int

            def __init__(self):
                self.n = 0

            def bump(self) -> int:
                """Adds one and gives the count."""
                self.n += 1
                return self.n

        let line = greet("x")
        let cleaned = line.strip()
        print(sorted([3, 1]), helper.aid())
        let args = sys.argv
        let sb = StringBuilder()
        sb.Append("x")
        sys.exit(0)

        """";

    public EditorDocumentationTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => TempDirectory.Remove(_directory);

    private string Write(string name, string text)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        return path;
    }

    private static string? Value(JsonNode? response) => response?["result"]?["contents"]?["value"]?.GetValue<string>()?.Replace("\r\n", "\n");

    [Fact]
    public void HoverShowsDocstringsAndDescriptions()
    {
        Write("helper.tpn", Helper);
        var main = Write("main.tpn", Main);
        var session = new LspSession().Open(main, Main)
            .Hover(main, Main, "greet(\"x\")")
            .Hover(main, Main, "Counter:")
            .Hover(main, Main, "bump(self)")
            .Hover(main, Main, "helper.aid")
            .Hover(main, Main, "strip()")
            .Hover(main, Main, "sorted(")
            .Hover(main, Main, "argv")
            .Hover(main, Main, "exit(0)")
            .Hover(main, Main, "Append(")
            .Run();
        var values = session.Responses().Select(Value).ToList();
        Assert.Equal("```taipan\ndef greet(name: str) -> str\n```\n\nSays hello to name.\n\nThe name is used as given.", values[0]);
        Assert.Equal("```taipan\nclass Counter\n```\n\nCounts up.", values[1]);
        Assert.Equal("```taipan\ndef Counter.bump(self) -> int\n```\n\nAdds one and gives the count.", values[2]);
        Assert.Equal("```taipan\nmodule helper\n```\n\nHelps out.", values[3]);
        Assert.Equal("```taipan\ns.strip()\ns.strip(chars)\n```\n\n`s.strip()` — `s` without whitespace at either end\n\n`s.strip(chars)` — `s` without any of the characters in `chars` at either end", values[4]);
        Assert.StartsWith("```taipan\nsorted(xs)\n", values[5]);
        Assert.Contains("smallest first", values[5]);
        Assert.Equal("```taipan\nsys.argv\n```\n\na `list[str]`: the program's path, then each of its arguments", values[6]);
        Assert.StartsWith("```taipan\nsys.exit()\nsys.exit(code)\nsys.exit(message)\n```\n\n", values[7]);
        Assert.StartsWith("```taipan\n.NET System.Text.StringBuilder.Append(", values[8]);
        Assert.Contains("```\n\nAppends", values[8]);
    }

    [Fact]
    public void CompletionAndSignatureHelpCarryDocumentation()
    {
        Write("helper.tpn", Helper);
        var main = Write("main.tpn", Main);
        var session = new LspSession().Open(main, Main)
            .Completion(main, Main, Main.IndexOf("line.strip", StringComparison.Ordinal) + "line.".Length)
            .SignatureHelp(main, Main, Main.IndexOf("sorted(", StringComparison.Ordinal) + "sorted(".Length)
            .SignatureHelp(main, Main, Main.IndexOf("strip()", StringComparison.Ordinal) + "strip(".Length)
            .SignatureHelp(main, Main, Main.IndexOf("greet(\"x\")", StringComparison.Ordinal) + "greet(".Length)
            .Completion(main, Main, Main.IndexOf("print(sorted", StringComparison.Ordinal) + "print(s".Length)
            .Run();
        var responses = session.Responses();

        var strip = responses[0]["result"]!["items"]!.AsArray().First(i => i!["label"]!.GetValue<string>() == "strip")!;
        Assert.Equal("s.strip()\ns.strip(chars)", strip["detail"]!.GetValue<string>());
        Assert.Contains("without whitespace", strip["documentation"]!["value"]!.GetValue<string>());

        var sorted = responses[1]["result"]!;
        Assert.Equal("sorted(xs)", sorted["signatures"]![0]!["label"]!.GetValue<string>());
        Assert.Contains("smallest first", sorted["signatures"]![0]!["documentation"]!["value"]!.GetValue<string>());

        var stripHelp = responses[2]["result"]!;
        Assert.Equal(["s.strip()", "s.strip(chars)"], stripHelp["signatures"]!.AsArray().Select(s => s!["label"]!.GetValue<string>()));
        Assert.Equal(0, stripHelp["activeSignature"]!.GetValue<int>());

        var greet = responses[3]["result"]!;
        Assert.Equal("Says hello to name.\n\nThe name is used as given.", greet["signatures"]![0]!["documentation"]!["value"]!.GetValue<string>());

        var sortedItem = responses[4]["result"]!["items"]!.AsArray().First(i => i!["label"]!.GetValue<string>() == "sorted")!;
        Assert.Contains("smallest first", sortedItem["documentation"]!["value"]!.GetValue<string>());
    }
}
