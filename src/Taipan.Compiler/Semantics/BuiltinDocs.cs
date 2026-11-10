using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Taipan.Compiler.Semantics;

/// <summary>A call form and what it gives, as the library reference lists it.</summary>
public sealed record BuiltinDoc(string Form, string Doc);

/// <summary>
/// The built-ins' documentation, shown by editors (SPEC §6, §16.3, M58): for each built-in function,
/// method and module member, its call forms and their descriptions, and for each exception class
/// what raises it. The sentences are the website's library reference's, and
/// <c>BuiltinDocsTests</c> keeps them the same, so this table is edited together with those pages.
/// Methods are keyed <c>str.strip</c>, <c>list.append</c>, <c>dict.get</c>, <c>set.add</c> and
/// <c>file.read</c>; module members <c>sys.argv</c>.
/// </summary>
public static class BuiltinDocs
{
    /// <summary>The built-in functions, by name.</summary>
    public static readonly ImmutableDictionary<string, ImmutableArray<BuiltinDoc>> Functions = new Dictionary<string, ImmutableArray<BuiltinDoc>>
    {
        ["abs"] = [new("abs(x)", "`x` without its sign: an `int` for an `int`, a `float` for a `float`; raises `OverflowError` for the smallest `int`")],
        ["all"] = [new("all(xs)", "`True` if every value is `True`; stops at the first `False`")],
        ["anext"] = [new("anext(it)", "the same for an async iterator, to be used with `await`; raises `StopAsyncIteration`"), new("anext(it, default)", "the same for an async iterator, to be used with `await`; raises `StopAsyncIteration`")],
        ["any"] = [new("any(xs)", "`True` if any value in an iterable of `bool`s is `True`; stops at the first `True`")],
        ["chr"] = [new("chr(n)", "a one-character `str`: the character with code point `n`; raises `ValueError` outside `0` to `1114111`")],
        ["dict"] = [new("dict(pairs)", "a new dict from an iterable of `(key, value)` tuples"), new("dict(d)", "a copy of the dict `d`"), new("dict(a=1, b=2)", "a new `dict[str, …]` from names and values"), new("dict()", "a new empty one, whose type has to be written, as in `let s: set[int] = set()`")],
        ["divmod"] = [new("divmod(a, b)", "the tuple `(a // b, a % b)`; raises `ZeroDivisionError` when `b` is zero")],
        ["enumerate"] = [new("enumerate(xs)", "an iterator of `(index, value)` tuples, counting from `0`"), new("enumerate(xs, start)", "the same, counting from `start`; also written `start=n`")],
        ["filter"] = [new("filter(f, xs)", "an iterator of the values in `xs` for which `f` gives `True`")],
        ["float"] = [new("float(x)", "for an `int`, the same number as a `float`"), new("float(s)", "for a `str`, a `float?`: the number the text spells, or `None` if it isn't one")],
        ["hash"] = [new("hash(x)", "an `int` that's equal for equal values; only for values that can be dict keys")],
        ["input"] = [new("input()", "a `str`: one line typed by the user, without its line ending; raises `EOFError` when there's nothing left to read"), new("input(prompt)", "the same, after writing `prompt` with no newline after it")],
        ["int"] = [new("int(x)", "for a `float`, the whole number part, dropping anything after the point; raises `OverflowError` if it's too big for an `int`, or infinite, and `ValueError` for NaN"), new("int(s)", "for a `str`, an `int?`: the number the text spells, or `None` if it isn't one")],
        ["isinstance"] = [new("isinstance(x, C)", "`True` if `x` is an object of the class `C`, or of a class derived from `C`; `False` for `None`"), new("isinstance(x, (A, B, …))", "`True` if `x` is an object of any of the classes listed")],
        ["iter"] = [new("iter(xs)", "an iterator over `xs`; for an iterator, the iterator itself")],
        ["len"] = [new("len(x)", "an `int`: the number of items in a list, dict, set or `range`, or of characters in a `str`")],
        ["list"] = [new("list(xs)", "a new list of every value in an iterable, in order"), new("list()", "a new empty one, whose type has to be written, as in `let s: set[int] = set()`")],
        ["map"] = [new("map(f, xs)", "an iterator of `f(x)` for each value `x` in `xs`")],
        ["max"] = [new("max(a, b, …)", "the smallest or largest of two or more values"), new("max(xs)", "the smallest or largest value in a list or other iterable; raises `ValueError` if it's empty"), new("max(xs, default=d)", "the same, but `d` if it's empty"), new("max(…, key=f)", "the value for which `f` gives the smallest or largest result")],
        ["min"] = [new("min(a, b, …)", "the smallest or largest of two or more values"), new("min(xs)", "the smallest or largest value in a list or other iterable; raises `ValueError` if it's empty"), new("min(xs, default=d)", "the same, but `d` if it's empty"), new("min(…, key=f)", "the value for which `f` gives the smallest or largest result")],
        ["next"] = [new("next(it)", "the next value from the iterator `it`; raises `StopIteration` when there's none"), new("next(it, default)", "the same, but `default` when there's none")],
        ["open"] = [new("open(path)", "a `TextIO`, an open text file; see [Files](../files/)"), new("open(path, mode)", "a `TextIO`, an open text file; see [Files](../files/)"), new("open(path)", "a `TextIO` for reading the file at `path`"), new("open(path, mode)", "a `TextIO` opened as `mode` says, below"), new("open(path, mode, encoding=e)", "the same, reading and writing text in the encoding `e`, `\"utf-8\"` if it isn't given"), new("open(path, mode, newline=\"\")", "the same, leaving line endings as they are")],
        ["ord"] = [new("ord(c)", "an `int`: the Unicode code point of a one-character `str`; raises `TypeError` for any other length")],
        ["print"] = [new("print(a, b, …)", "nothing; writes each value's text, with a space between each and a newline at the end"), new("print(…, sep=s)", "the same, with `s` between the values instead of a space"), new("print(…, end=s)", "the same, with `s` at the end instead of a newline"), new("print(…, file=sys.stderr)", "the same, written to the error stream instead of the screen; needs `import sys`"), new("print(…, flush=True)", "the same, sent out at once rather than when the buffer fills"), new("print()", "nothing; writes an empty line")],
        ["range"] = [new("range(stop)", "the whole numbers `0`, `1`, … up to, not including, `stop`"), new("range(start, stop)", "the whole numbers from `start` up to, not including, `stop`"), new("range(start, stop, step)", "the same, counting by `step`; a negative `step` counts down; raises `ValueError` when `step` is `0`")],
        ["repr"] = [new("repr(x)", "the text a value has inside a list: a `str` in double quotes, with special characters such as tabs and newlines written as `\\t` and `\\n`")],
        ["reversed"] = [new("reversed(xs)", "an iterator over a list, `str`, dict's keys, `range` or `tuple[T, ...]`, from the last item to the first")],
        ["round"] = [new("round(x)", "the nearest `int`; a value exactly halfway goes to the even one, so `round(2.5)` is `2`; raises `ValueError` for NaN and `OverflowError` for infinity"), new("round(x, n)", "`x` rounded to `n` digits after the point, as a `float` for a `float`; for an `int`, a negative `n` rounds to tens, hundreds and so on")],
        ["set"] = [new("set(xs)", "a new set of the distinct values in an iterable"), new("set()", "a new empty one, whose type has to be written, as in `let s: set[int] = set()`")],
        ["sorted"] = [new("sorted(xs)", "a new list of the values in `xs`, smallest first"), new("sorted(xs, key=f)", "the same, ordered by what `f` gives for each value"), new("sorted(xs, reverse=True)", "the same, largest first")],
        ["str"] = [new("str(x)", "the text `print` would write for any value")],
        ["sum"] = [new("sum(xs)", "the total of a list or other iterable of numbers: an `int` for `int`s, a `float` if any is a `float`"), new("sum(xs, start)", "the same, added to `start` instead of `0`")],
        ["super"] = [new("super().m(…)", "calls the method `m` as the base class defines it, from inside a method of a derived class")],
        ["tuple"] = [new("tuple(xs)", "a new tuple of every value in an iterable")],
        ["zip"] = [new("zip(xs, ys, …)", "an iterator of tuples, one value from each of two or more iterables, stopping at the shortest")],
    }.ToImmutableDictionary();

    /// <summary>The built-in types' methods, by type and name.</summary>
    public static readonly ImmutableDictionary<string, ImmutableArray<BuiltinDoc>> Methods = new Dictionary<string, ImmutableArray<BuiltinDoc>>
    {
        ["dict.clear"] = [new("d.clear()", "removes every key")],
        ["dict.copy"] = [new("d.copy()", "a new dict with the same keys and values")],
        ["dict.get"] = [new("d.get(k)", "the value for `k` as an optional, `None` if there's none"), new("d.get(k, default)", "the value for `k`, or `default` if there's none")],
        ["dict.items"] = [new("d.items()", "a new `list[tuple[K, V]]` of the keys and values, in order")],
        ["dict.keys"] = [new("d.keys()", "a new list of the keys, or of the values, in order")],
        ["dict.pop"] = [new("d.pop(k)", "removes `k` and gives its value; raises `KeyError` if it's not there"), new("d.pop(k, default)", "the same, but gives `default` if `k` isn't there")],
        ["dict.remove"] = [new("d.remove(k)", "removes `k` if it's there, and gives whether it was; Taipan's own, not Python's")],
        ["dict.setdefault"] = [new("d.setdefault(k, default)", "the value for `k`, first adding `k` with `default` if it isn't there")],
        ["dict.update"] = [new("d.update(other)", "adds or replaces every key and value from `other`")],
        ["dict.values"] = [new("d.values()", "a new list of the keys, or of the values, in order")],
        ["file.close"] = [new("f.close()", "flushes and closes the file; closing a closed file does nothing")],
        ["file.flush"] = [new("f.flush()", "writes out anything still held in memory, so other programs can see it")],
        ["file.read"] = [new("f.read()", "the rest of the file, as one `str`; `\"\"` at the end"), new("f.read(n)", "at most the next `n` characters; `\"\"` at the end")],
        ["file.readline"] = [new("f.readline()", "the next line, with its line ending; `\"\"` at the end")],
        ["file.readlines"] = [new("f.readlines()", "the rest of the file as a `list[str]` of lines, each with its line ending")],
        ["file.write"] = [new("f.write(s)", "writes `s`, and gives the number of characters written")],
        ["file.writelines"] = [new("f.writelines(lines)", "writes each string in `lines`, a list or any other iterable, adding nothing between them")],
        ["list.append"] = [new("xs.append(x)", "adds `x` to the end")],
        ["list.clear"] = [new("xs.clear()", "removes every element")],
        ["list.contains"] = [new("xs.contains(x)", "whether `xs` has an element equal to `x`, as `x in xs` does; Taipan's own, not Python's")],
        ["list.copy"] = [new("xs.copy()", "a new list with the same elements")],
        ["list.count"] = [new("xs.count(x)", "how many elements are equal to `x`")],
        ["list.extend"] = [new("xs.extend(ys)", "adds every value of `ys`, which can be a list, a set, a range or anything else a `for` loop can go over, to the end")],
        ["list.index"] = [new("xs.index(x)", "the position of the first element equal to `x`; raises `ValueError` if there's none")],
        ["list.insert"] = [new("xs.insert(i, x)", "puts `x` before position `i`; a negative `i` counts from the end, and an `i` past either end puts `x` at that end")],
        ["list.pop"] = [new("xs.pop()", "removes the last element and gives it; raises `IndexError` if the list is empty"), new("xs.pop(i)", "removes the element at `i` and gives it; a negative `i` counts from the end; raises `IndexError` if `i` is out of range")],
        ["list.remove"] = [new("xs.remove(x)", "removes the first element equal to `x`; raises `ValueError` if there's none")],
        ["list.reverse"] = [new("xs.reverse()", "reverses the order of the elements, in place")],
        ["list.sort"] = [new("xs.sort()", "sorts the elements in place, from smallest (see [Sorting](#sorting))"), new("xs.sort(key=f, reverse=True)", "sorts in place by what `f` gives for each element, and from largest with `reverse=True`")],
        ["set.add"] = [new("s.add(x)", "adds `x`, if it isn't there already")],
        ["set.clear"] = [new("s.clear()", "removes every value")],
        ["set.copy"] = [new("s.copy()", "a new set with the same values")],
        ["set.difference"] = [new("s.difference(xs)", "a new set of the values in `s` but not in `xs`")],
        ["set.discard"] = [new("s.discard(x)", "removes `x` if it's there, and does nothing if it isn't")],
        ["set.intersection"] = [new("s.intersection(xs)", "a new set of the values in both `s` and `xs`")],
        ["set.isdisjoint"] = [new("s.isdisjoint(xs)", "whether `s` and `xs` have no value in common")],
        ["set.issubset"] = [new("s.issubset(xs)", "whether every value in `s` is in `xs`")],
        ["set.issuperset"] = [new("s.issuperset(xs)", "whether every value in `xs` is in `s`")],
        ["set.pop"] = [new("s.pop()", "removes a value and gives it: the first one added of those still there; raises `KeyError` if the set is empty")],
        ["set.remove"] = [new("s.remove(x)", "removes `x`; raises `KeyError` if it isn't there")],
        ["set.symmetric_difference"] = [new("s.symmetric_difference(xs)", "a new set of the values in exactly one of `s` and `xs`")],
        ["set.union"] = [new("s.union(xs)", "a new set of the values in `s`, `xs` or both")],
        ["set.update"] = [new("s.update(xs)", "adds every value of `xs`, which can be any collection")],
        ["str.capitalize"] = [new("s.capitalize()", "the first character a capital, the rest lowercase")],
        ["str.center"] = [new("s.center(width)", "`s` padded with spaces to `width` characters, on the right, on the left, or on both sides")],
        ["str.count"] = [new("s.count(sub)", "how many times `sub` appears, without overlaps")],
        ["str.endswith"] = [new("s.endswith(p)", "whether `s` starts or ends with `p`")],
        ["str.find"] = [new("s.find(sub)", "the index of the first `sub` in `s`, or `-1` if there's none")],
        ["str.index"] = [new("s.index(sub)", "the index of the first `sub`; raises `ValueError` if there's none")],
        ["str.isalnum"] = [new("s.isalnum()", "every character is a letter or a digit")],
        ["str.isalpha"] = [new("s.isalpha()", "every character is a letter")],
        ["str.isdigit"] = [new("s.isdigit()", "every character is a digit")],
        ["str.islower"] = [new("s.islower()", "it has letters, and they're all uppercase, or all lowercase")],
        ["str.isspace"] = [new("s.isspace()", "every character is whitespace")],
        ["str.isupper"] = [new("s.isupper()", "it has letters, and they're all uppercase, or all lowercase")],
        ["str.join"] = [new("sep.join(xs)", "the strings in `xs` joined, with `sep` between each")],
        ["str.ljust"] = [new("s.ljust(width)", "`s` padded with spaces to `width` characters, on the right, on the left, or on both sides"), new("s.ljust(width, fill)", "the same, padded with the character `fill`")],
        ["str.lower"] = [new("s.lower()", "`s` in lowercase or uppercase")],
        ["str.lstrip"] = [new("s.lstrip()", "the same, at the start only"), new("s.lstrip(chars)", "the same, at the start only")],
        ["str.partition"] = [new("s.partition(sep)", "a `tuple[str, str, str]`: the part before the first `sep`, `sep`, and the part after; `(s, \"\", \"\")` if there's no `sep`")],
        ["str.removeprefix"] = [new("s.removeprefix(p)", "`s` without `p` at the start, if it's there")],
        ["str.removesuffix"] = [new("s.removesuffix(p)", "`s` without `p` at the end, if it's there")],
        ["str.replace"] = [new("s.replace(old, new)", "`s` with every `old` replaced by `new`")],
        ["str.rfind"] = [new("s.rfind(sub)", "the index of the last `sub`, or `-1`")],
        ["str.rjust"] = [new("s.rjust(width)", "`s` padded with spaces to `width` characters, on the right, on the left, or on both sides")],
        ["str.rpartition"] = [new("s.rpartition(sep)", "the same, at the last `sep`; `(\"\", \"\", s)` if there's no `sep`")],
        ["str.rsplit"] = [new("s.rsplit(sep, maxsplit)", "the same, from the right")],
        ["str.rstrip"] = [new("s.rstrip()", "the same, at the end only"), new("s.rstrip(chars)", "the same, at the end only")],
        ["str.split"] = [new("s.split()", "a `list[str]` of the words in `s`, split on runs of spaces, tabs and newlines, with none empty"), new("s.split(sep)", "a `list[str]` of the parts between each `sep`; parts may be empty"), new("s.split(sep, maxsplit)", "the same, splitting at most `maxsplit` times, from the left")],
        ["str.splitlines"] = [new("s.splitlines()", "a `list[str]` of the lines in `s`, without their line endings")],
        ["str.startswith"] = [new("s.startswith(p)", "whether `s` starts or ends with `p`")],
        ["str.strip"] = [new("s.strip()", "`s` without whitespace at either end"), new("s.strip(chars)", "`s` without any of the characters in `chars` at either end")],
        ["str.swapcase"] = [new("s.swapcase()", "lowercase made uppercase, and uppercase lowercase")],
        ["str.title"] = [new("s.title()", "each word starting with a capital, the rest lowercase")],
        ["str.upper"] = [new("s.upper()", "`s` in lowercase or uppercase")],
        ["str.zfill"] = [new("s.zfill(width)", "`s` padded with zeros on the left, after any sign")],
    }.ToImmutableDictionary();

    /// <summary>The built-in modules' members, by module and name.</summary>
    public static readonly ImmutableDictionary<string, ImmutableArray<BuiltinDoc>> Members = new Dictionary<string, ImmutableArray<BuiltinDoc>>
    {
        ["asyncio.CancelledError"] = [new("asyncio.CancelledError", "the exception that stops an async function that's cancelled")],
        ["asyncio.TimeoutError"] = [new("asyncio.TimeoutError", "another name for the built-in `TimeoutError`")],
        ["asyncio.gather"] = [new("asyncio.gather(a, b, …)", "an awaitable that waits for every one of `a`, `b`, … and gives a list of their results, in the order they were given"), new("asyncio.gather(*aws)", "the same, for each awaitable in the list `aws`")],
        ["asyncio.run"] = [new("asyncio.run(aw)", "runs `aw` to its end and gives its result")],
        ["asyncio.sleep"] = [new("asyncio.sleep(seconds)", "an awaitable that finishes after `seconds`, an `int` or `float`")],
        ["asyncio.timeout"] = [new("asyncio.timeout(seconds)", "a limit for `async with`: the code inside is stopped after `seconds`, and `TimeoutError` raised")],
        ["asyncio.wait_for"] = [new("asyncio.wait_for(aw, seconds)", "an awaitable that gives `aw`'s result, or raises `TimeoutError` if `aw` takes longer than `seconds`")],
        ["os.environ"] = [new("os.environ", "a `dict[str, str]` of the environment variables' names and values")],
        ["os.getenv"] = [new("os.getenv(name)", "the value of the variable `name`, as a `str?`: `None` if it isn't set"), new("os.getenv(name, default)", "the value of `name`, or `default` if it isn't set, as a `str`")],
        ["sys.argv"] = [new("sys.argv", "a `list[str]`: the program's path, then each of its arguments")],
        ["sys.exit"] = [new("sys.exit()", "ends the program with exit code 0"), new("sys.exit(code)", "ends the program with the exit code `code`, an `int`"), new("sys.exit(message)", "writes `message`, a `str`, to the error output, and ends the program with exit code 1")],
        ["sys.stderr"] = [new("sys.stderr", "the program's error output, as a file: where error messages go")],
        ["sys.stdin"] = [new("sys.stdin", "the program's input, as a file")],
        ["sys.stdout"] = [new("sys.stdout", "the program's output, as a file: where `print` writes")],
    }.ToImmutableDictionary();

    /// <summary>What raises each exception class.</summary>
    public static readonly ImmutableDictionary<string, string> Exceptions = new Dictionary<string, string>
    {
        ["ArithmeticError"] = "nothing directly: the base of `ZeroDivisionError` and `OverflowError`, for catching both",
        ["AssertionError"] = "an `assert` whose condition is `False`",
        ["BaseException"] = "nothing directly: the base of every exception class, for catching all of them",
        ["CancelledError"] = "cancelling an async function (see [asyncio](../asyncio/))",
        ["DotNetError"] = "an exception in .NET code the program calls, when it's caught as a Taipan class (below)",
        ["EOFError"] = "`input()` when the input has ended",
        ["Exception"] = "nothing directly: the base of every class but `SystemExit`, `GeneratorExit` and `asyncio.CancelledError`, for catching any error",
        ["FileExistsError"] = "opening a file that already exists, with mode `\"x\"`",
        ["FileNotFoundError"] = "opening a file that doesn't exist, for reading",
        ["GeneratorExit"] = "a generator's `close()`, inside the generator",
        ["IndexError"] = "an index past the end of a list, string, tuple or range, and `pop()` on an empty list",
        ["IsADirectoryError"] = "opening a directory as a file",
        ["KeyError"] = "a key missing from a dict, a value missing in a set's `remove`, and `pop()` on an empty set",
        ["LookupError"] = "nothing directly: the base of `IndexError` and `KeyError`, for catching both",
        ["NameError"] = "a function reading a top-level variable before it's set, where the compiler can't tell",
        ["OSError"] = "opening a file the system refuses to open, reading a file opened for writing, and writing one opened for reading (see [Files](../files/))",
        ["OverflowError"] = "an `int` result too large for 64 bits, and `int(x)` of an infinite `float`",
        ["PermissionError"] = "opening a file the program may not read or write",
        ["PythonError"] = "an exception in Python code the program calls (below)",
        ["RuntimeError"] = "a list, dict or set that gains or loses elements while a loop goes through it",
        ["StopAsyncIteration"] = "the same, for an async iterator",
        ["StopIteration"] = "`next(it)` when the iterator `it` has nothing left",
        ["SystemExit"] = "`sys.exit` (see [sys and os](../sys-and-os/))",
        ["TimeoutError"] = "`asyncio.wait_for` and `asyncio.timeout` running out of time",
        ["TypeError"] = "an `as` conversion that fails, and `ord(s)` of a string that isn't one character",
        ["UnicodeDecodeError"] = "reading bytes that aren't valid in a file's encoding (see [Files](../files/))",
        ["UnicodeEncodeError"] = "writing a character a file's encoding can't hold",
        ["UnicodeError"] = "nothing directly: the base of `UnicodeDecodeError` and `UnicodeEncodeError`, for catching both",
        ["ValueError"] = "a value of the right type that's wrong: `int(x)` of a NaN, `min` or `max` of nothing, `xs.remove(x)` or `xs.index(x)` with no `x` in `xs`, `s.split(\"\")`, a slice or range step of 0",
        ["ZeroDivisionError"] = "dividing by zero, with `/`, `//` or `%`",
    }.ToImmutableDictionary();

    /// <summary>The built-in types, in a sentence each.</summary>
    public static readonly ImmutableDictionary<string, string> Types = new Dictionary<string, string>
    {
        ["str"] = "Text: a sequence of characters, which can't be changed.",
        ["int"] = "A 64-bit whole number; arithmetic that overflows raises `OverflowError`.",
        ["float"] = "A 64-bit floating-point number.",
        ["bool"] = "`True` or `False`.",
        ["list"] = "An ordered collection that can change, of one element type.",
        ["dict"] = "Keys mapped to values, in the order the keys were added.",
        ["set"] = "A collection of distinct values, with no order to rely on.",
        ["tuple"] = "A fixed group of values, each with its own type.",
        ["range"] = "The whole numbers from a start up to, not including, a stop.",
        ["TextIO"] = "An open text file, or one of the standard streams.",
        ["object"] = "Any value: the type every other type is assignable to.",
        ["py"] = "A Python value, checked by Python when the program runs.",
    }.ToImmutableDictionary();

    /// <summary>The links in a description, reduced to their text, since an editor can't follow them.</summary>
    public static string Plain(string doc) => Regex.Replace(doc, @"\[([^\]]+)\]\([^)]*\)", "$1");
}
