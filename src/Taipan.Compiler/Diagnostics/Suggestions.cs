namespace Taipan.Compiler.Diagnostics;

/// <summary>
/// "Did you mean" help for a name that isn't found (SPEC §9, M57): the closest visible name when
/// one is within a couple of edits, and the Taipan spelling of a method name from another language.
/// </summary>
public static class Suggestions
{
    /// <summary>The closest of <paramref name="candidates"/> to <paramref name="name"/>, or null when none is close.</summary>
    public static string? Closest(string name, IEnumerable<string> candidates)
    {
        if (name.Length < 3)
            return null;
        var limit = name.Length < 5 ? 1 : 2;
        string? best = null;
        var bestDistance = limit + 1;
        foreach (var candidate in candidates.Distinct().Order(StringComparer.Ordinal))
        {
            if (candidate == name || candidate.StartsWith("__") || Math.Abs(candidate.Length - name.Length) > limit)
                continue;
            var distance = Distance(name, candidate);
            if (distance < bestDistance)
                (best, bestDistance) = (candidate, distance);
        }
        return best;
    }

    /// <summary>Help for a method <paramref name="name"/> that <paramref name="known"/> doesn't contain, on a <paramref name="kind"/> (str, list, …).</summary>
    public static string? Method(string kind, string name, IEnumerable<string> known)
    {
        var other = (kind, name) switch
        {
            ("list", "push" or "add" or "pushBack" or "push_back") => "write 'xs.append(x)'",
            ("list" or "str" or "dict" or "set", "length" or "size" or "count" or "Count" or "Length") when name is not "count" || kind is "dict" or "set" => "write 'len(x)'",
            ("list" or "str", "indexOf" or "IndexOf") => "write 'x.index(v)', or 'x.find(v)' on a str",
            ("list" or "dict" or "set" or "str", "contains" or "Contains" or "has" or "includes") => "write 'v in x'",
            ("str", "trim" or "Trim") => "write 's.strip()'",
            ("str", "toUpperCase" or "toUpper" or "ToUpper") => "write 's.upper()'",
            ("str", "toLowerCase" or "toLower" or "ToLower") => "write 's.lower()'",
            ("str", "startsWith" or "StartsWith") => "write 's.startswith(p)'",
            ("str", "endsWith" or "EndsWith") => "write 's.endswith(p)'",
            ("str", "substring" or "Substring" or "slice") => "slice it: 's[start:end]'",
            ("str", "charAt") => "index it: 's[i]'",
            ("str", "replaceAll") => "write 's.replace(old, new)', which replaces every occurrence",
            ("list", "forEach" or "ForEach") => "write 'for x in xs:'",
            ("list", "map" or "filter") => $"write '{name}(f, xs)', or a comprehension",
            ("dict", "containsKey" or "ContainsKey" or "has" or "hasKey") => "write 'k in d'",
            ("list", "sorted") => "write 'sorted(xs)', or 'xs.sort()' to sort in place",
            _ => null,
        };
        if (other is not null)
            return other;
        return Closest(name, known) is { } close ? $"did you mean '{close}'?" : null;
    }

    public static string? DidYouMean(string name, IEnumerable<string> candidates) =>
        Closest(name, candidates) is { } close ? $"did you mean '{close}'?" : null;

    // Levenshtein distance, case-insensitive on the first character so that `Print` finds `print`.
    private static int Distance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
            previous[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = char.ToLowerInvariant(a[i - 1]) == char.ToLowerInvariant(b[j - 1]) ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }
            (previous, current) = (current, previous);
        }
        return previous[b.Length];
    }
}
