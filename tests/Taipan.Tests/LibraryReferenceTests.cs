using System.Text.RegularExpressions;
using Taipan.Compiler.Lexing;
using Taipan.Compiler.Semantics;
using Taipan.Compiler.Syntax;
using Taipan.Compiler.Text;

namespace Taipan.Tests;

/// <summary>
/// The library reference lists every method the compiler accepts, and only those (M56). For each
/// type, the candidates are Python's public methods on it and Taipan's own additions. A candidate
/// is accepted when a call to it binds without "has no method" (TP0210), whatever its arguments.
/// A method counts as documented when the first column of a table row on the type's page shows a
/// call to it, as in <c>`s.strip()`</c>.
/// </summary>
public class LibraryReferenceTests
{
    private static readonly string Library = Path.Combine(RepoPaths.Root, "website", "src", "content", "docs", "library");

    /// <summary>A type: its page, a program that declares <c>x</c> of it with METHOD where the call goes, and its candidates.</summary>
    private sealed record Kind(string Page, string Probe, string[] Candidates);

    // Python 3.12's public methods on each type (dir() without the dunders), and then Taipan's own.
    private static readonly Dictionary<string, Kind> Kinds = new()
    {
        ["str"] = new("str.md", "let x = \"a\"\nx.METHOD()\n",
            [.. Words("capitalize casefold center count encode endswith expandtabs find format format_map index isalnum isalpha isascii isdecimal isdigit isidentifier islower isnumeric isprintable isspace istitle isupper join ljust lower lstrip maketrans partition removeprefix removesuffix replace rfind rindex rjust rpartition rsplit rstrip split splitlines startswith strip swapcase title translate upper zfill")]),
        ["list"] = new("list.md", "let x = [1]\nx.METHOD()\n",
            [.. Words("append clear copy count extend index insert pop remove reverse sort"), "contains"]),
        ["dict"] = new("dict.md", "let x = {\"a\": 1}\nx.METHOD()\n",
            [.. Words("clear copy fromkeys get items keys pop popitem setdefault update values"), "remove"]),
        ["set"] = new("set.md", "let x = {1}\nx.METHOD()\n",
            [.. Words("add clear copy difference difference_update discard intersection intersection_update isdisjoint issubset issuperset pop remove symmetric_difference symmetric_difference_update union update")]),
        ["file"] = new("files.md", "with open(\"f.txt\") as x:\n    x.METHOD()\n",
            [.. Words("close detach fileno flush isatty read readable readline readlines reconfigure seek seekable tell truncate writable write writelines")]),
    };

    public static TheoryData<string> Types() => [.. Kinds.Keys];

    /// <summary>Every method the compiler accepts on each type, for the documentation's coverage (M58).</summary>
    internal static IEnumerable<(string Kind, string Method)> AcceptedMethods() =>
        Kinds.SelectMany(k => k.Value.Candidates.Where(m => Accepted(k.Value, m)).Select(m => (k.Key, m)));

    [Theory]
    [MemberData(nameof(Types))]
    public void EveryAcceptedMethodIsDocumented(string type)
    {
        var kind = Kinds[type];
        var documented = Documented(kind.Page);
        var missing = kind.Candidates.Where(m => Accepted(kind, m) && !documented.Contains(m)).ToList();
        Assert.True(missing.Count == 0, $"{kind.Page} doesn't list: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(Types))]
    public void EveryDocumentedMethodIsAccepted(string type)
    {
        var kind = Kinds[type];
        var wrong = Documented(kind.Page).Where(m => !Accepted(kind, m)).ToList();
        Assert.True(wrong.Count == 0, $"{kind.Page} lists methods the compiler rejects: {string.Join(", ", wrong)}");
    }

    // A call in the first cell of a table row: `s.strip(chars)`, `sep.join(xs)`.
    private static readonly Regex Call = new(@"`[^`]*?\b\w+\.(?<name>\w+)\(");

    private static HashSet<string> Documented(string page)
    {
        var path = Path.Combine(Library, page);
        Assert.True(File.Exists(path), $"no page {path}");
        var names = new HashSet<string>();
        foreach (var line in File.ReadAllLines(path).Where(l => l.StartsWith("| `")))
        {
            var first = line.Split('|')[1];
            foreach (Match m in Call.Matches(first))
                names.Add(m.Groups["name"].Value);
        }
        return names;
    }

    private static bool Accepted(Kind kind, string method)
    {
        var source = SourceText.From(kind.Probe.Replace("METHOD", method), "probe.tpn");
        var lexed = Lexer.Lex(source);
        var parsed = Parser.Parse(source, lexed.Tokens);
        var bound = Binder.Bind(source, parsed.Unit);
        return !bound.Diagnostics.Any(d => d.Code == "TP0210" && d.Message.Contains($"'{method}'"));
    }

    private static string[] Words(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
