using System.Text.RegularExpressions;
using Taipan.Compiler.Semantics;

namespace Taipan.Tests;

/// <summary>
/// The built-ins' documentation is the library reference's (SPEC §6, M58): every built-in function,
/// method, module member and exception class is in <see cref="BuiltinDocs"/>, and each entry's call
/// form and description are a row of the page for it, so the editor and the website say the same.
/// </summary>
public class BuiltinDocsTests
{
    private static readonly string Library = Path.Combine(RepoPaths.Root, "website", "src", "content", "docs", "library");

    // A page's table rows: each `form` in the first cell, with the second cell.
    private static List<(string Form, string Doc)> Rows(params string[] pages)
    {
        var rows = new List<(string, string)>();
        foreach (var page in pages)
        {
            foreach (var line in File.ReadAllLines(Path.Combine(Library, page)).Where(l => l.StartsWith("| `")))
            {
                var cells = Regex.Split(line.Trim()[1..^1], @"(?<!\\)\|").Select(c => c.Trim()).ToList();
                if (cells.Count < 2)
                    continue;
                foreach (Match form in Regex.Matches(cells[0], @"`([^`]+)`"))
                    rows.Add((form.Groups[1].Value, cells[1].Replace("\\|", "|")));
            }
        }
        return rows;
    }

    private static string PageOf(string kind) => kind switch { "file" => "files.md", _ => $"{kind}.md" };

    [Fact]
    public void EveryMethodEntryIsARowOfItsPage()
    {
        foreach (var (key, forms) in BuiltinDocs.Methods)
        {
            var rows = Rows(PageOf(key.Split('.')[0]));
            foreach (var form in forms)
                Assert.Contains((form.Form, form.Doc), rows);
        }
    }

    [Fact]
    public void EveryFunctionMemberAndExceptionEntryIsARow()
    {
        var functions = Rows("functions.md", "files.md");
        foreach (var forms in BuiltinDocs.Functions.Values)
            foreach (var form in forms)
                Assert.Contains((form.Form, form.Doc), functions);
        var members = Rows("sys-and-os.md", "asyncio.md");
        foreach (var forms in BuiltinDocs.Members.Values)
            foreach (var form in forms)
                Assert.Contains((form.Form, form.Doc), members);
        var exceptions = Rows("exceptions.md").Select(r => (r.Form.Replace("asyncio.", ""), r.Doc)).ToList();
        foreach (var (name, doc) in BuiltinDocs.Exceptions)
            Assert.Contains((name, doc), exceptions);
    }

    [Fact]
    public void EveryBuiltinIsDocumented()
    {
        var functions = FunctionSymbol.CreateBuiltins().Select(f => f.Name).Where(n => !BuiltinDocs.Functions.ContainsKey(n)).ToList();
        Assert.True(functions.Count == 0, $"functions without documentation: {string.Join(", ", functions)}");

        var methods = LibraryReferenceTests.AcceptedMethods().Select(m => $"{m.Kind}.{m.Method}").Where(k => !BuiltinDocs.Methods.ContainsKey(k)).ToList();
        Assert.True(methods.Count == 0, $"methods without documentation: {string.Join(", ", methods)}");

        // A module's classes (asyncio's exceptions and its Timeout) are documented as classes, not members.
        var members = new[] { "sys", "os", "asyncio" }.SelectMany(m => BuiltinModules.MemberNames(m).Where(n => char.IsLower(n[0])).Select(n => $"{m}.{n}"))
            .Where(k => !BuiltinDocs.Members.ContainsKey(k)).ToList();
        Assert.True(members.Count == 0, $"module members without documentation: {string.Join(", ", members)}");

        var exceptions = BuiltinClasses.All.Select(c => c.Name).Where(n => !BuiltinDocs.Exceptions.ContainsKey(n)).ToList();
        Assert.True(exceptions.Count == 0, $"exception classes without documentation: {string.Join(", ", exceptions)}");
    }
}
