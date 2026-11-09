using System.Text.RegularExpressions;

namespace Taipan.Tests;

/// <summary>
/// Every relative link on the site's hand-written pages leads to a page (M56). A page is served at
/// its path without the extension, with a trailing slash, and <c>index.md</c> at its directory, so a
/// link resolves against that. The generated pages aren't in the repository: a link to
/// <c>diagnostics/tp0203/</c> or <c>diagnostics/tpn002/</c> must name a code in SPEC §9, and one to
/// the specification is taken as it is. Anchors aren't checked.
/// </summary>
public class WebsiteLinkTests
{
    private static readonly string Docs = Path.Combine(RepoPaths.Root, "website", "src", "content", "docs");
    private static readonly string[] Generated = ["diagnostics", "reference/specification"];

    // A Markdown link's target, outside code: [text](target) or [text](target "title").
    private static readonly Regex Link = new(@"\]\((?<target>[^)\s]+)(?:\s+""[^""]*"")?\)");
    private static readonly Regex Code = new(@"```.*?```|`[^`\n]*`", RegexOptions.Singleline);

    // The diagnostics' explanations (M56) appear on diagnostics/<code>/, so their links resolve from there.
    private static readonly string Explanations = Path.Combine(RepoPaths.Root, "website", "diagnostics");

    public static TheoryData<string> Pages() =>
    [
        .. Directory.EnumerateFiles(Docs, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".md") || f.EndsWith(".mdx"))
            .Select(f => Path.GetRelativePath(Docs, f).Replace('\\', '/'))
            .Where(f => !Generated.Any(g => f.StartsWith(g)))
            .Concat(Directory.Exists(Explanations)
                ? Directory.EnumerateFiles(Explanations, "*.md").Select(f => $"diagnostics/{Path.GetFileName(f)}")
                : [])
            .Order(StringComparer.Ordinal),
    ];

    [Theory]
    [MemberData(nameof(Pages))]
    public void RelativeLinksLeadToPages(string page)
    {
        var file = page.StartsWith("diagnostics/") ? Path.Combine(Explanations, page["diagnostics/".Length..]) : Path.Combine(Docs, page);
        var text = Code.Replace(File.ReadAllText(file), "");
        var broken = Link.Matches(text)
            .Select(m => m.Groups["target"].Value)
            .Where(t => !t.Contains("://") && !t.StartsWith('#') && !t.StartsWith("mailto:"))
            .Where(t => !Resolves(page, t))
            .ToList();
        Assert.True(broken.Count == 0, $"{page} links to missing pages: {string.Join(", ", broken)}");
    }

    private static bool Resolves(string page, string target)
    {
        var path = target.Split('#')[0];
        if (path == "")
            return true;
        // The page's own URL directory: guides/python.md is served at guides/python/.
        var name = Path.GetFileNameWithoutExtension(page);
        var dir = Path.GetDirectoryName(page)!.Replace('\\', '/');
        var url = name == "index" ? dir : (dir == "" ? name : $"{dir}/{name}");
        var segments = (path.StartsWith('/') ? path : $"{url}/{path}").Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        var resolved = new List<string>();
        foreach (var segment in segments)
        {
            if (segment == "..")
            {
                if (resolved.Count == 0)
                    return false;
                resolved.RemoveAt(resolved.Count - 1);
            }
            else if (segment != ".")
                resolved.Add(segment);
        }
        var to = string.Join('/', resolved);
        if (to == "reference/specification")
            return true;
        if (to == "diagnostics")
            return true;
        if (to.StartsWith("diagnostics/"))
            return Codes.Value.Contains(to["diagnostics/".Length..].ToUpperInvariant());
        return new[] { $"{to}.md", $"{to}.mdx", $"{to}/index.md", $"{to}/index.mdx" }
            .Any(f => File.Exists(Path.Combine(Docs, f)));
    }

    private static readonly Lazy<HashSet<string>> Codes = new(() =>
        [.. Regex.Matches(File.ReadAllText(Path.Combine(RepoPaths.Root, "SPEC.md")), @"^\| (TPN?\d{3,4}) \|", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)]);
}
