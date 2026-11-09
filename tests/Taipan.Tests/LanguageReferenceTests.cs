using System.Text.RegularExpressions;
using Taipan.Compiler.Lexing;

namespace Taipan.Tests;

/// <summary>
/// The language reference covers every keyword (M56): each one the lexer knows appears in code, in
/// backticks or a <c>taipan</c> block, on at least one page in <c>language/</c>.
/// </summary>
public class LanguageReferenceTests
{
    private static readonly string Language = Path.Combine(RepoPaths.Root, "website", "src", "content", "docs", "language");

    private static readonly Regex Code = new(@"```taipan\n(?<block>.*?)```|`(?<span>[^`\n]+)`", RegexOptions.Singleline);
    private static readonly Regex Word = new(@"\b[A-Za-z_]+\b");

    [Fact]
    public void EveryKeywordIsDocumented()
    {
        Assert.True(Directory.Exists(Language), $"no directory {Language}");
        var words = new HashSet<string>();
        foreach (var page in Directory.EnumerateFiles(Language, "*.md"))
            foreach (Match code in Code.Matches(File.ReadAllText(page).Replace("\r\n", "\n")))
                foreach (Match word in Word.Matches(code.Value))
                    words.Add(word.Value);
        var missing = TokenFacts.Keywords.Keys.Where(k => !words.Contains(k)).Order(StringComparer.Ordinal).ToList();
        Assert.True(missing.Count == 0, $"no language page shows: {string.Join(", ", missing)}");
    }
}
