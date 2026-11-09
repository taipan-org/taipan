---
title: Built-ins
description: Built-in functions, methods, modules and exception classes.
sidebar:
  order: 2
---

Taipan's built-ins follow Python's names and behavior, with types. The [specification](../specification/#6-built-ins-v01) gives every rule.

## Functions

| Function | Notes |
|---|---|
| `print(*args, sep=" ", end="\n", file=sys.stdout, flush=False)` | |
| `input()`, `input(prompt)` | raises `EOFError` at the end of input |
| `open(file, mode="r", *, encoding="utf-8", newline=None)` | text files; modes `r`, `w`, `a`, `x` |
| `len(x)` | lists, dicts, sets and strings (in code points) |
| `range(stop)`, `range(start, stop[, step])` | in `for` loops |
| `str(x)`, `repr(x)`, `hash(x)` | |
| `int(x)`, `float(x)` | from a number; from a `str`, an optional: `None` if it isn't a number |
| `abs`, `round`, `divmod`, `min`, `max`, `sum` | `min` and `max` take `key=` |
| `sorted(xs, key=…, reverse=False)` | stable |
| `any(xs)`, `all(xs)` | of `bool`s |
| `map`, `filter`, `zip`, `enumerate` | lazy iterators |
| `list(xs)`, `set(xs)`, `iter(xs)`, `next(it[, default])`, `anext(it[, default])` | |
| `isinstance(x, C)`, `isinstance(x, (A, B))` | narrows `x` |
| `super()` | as `super().method(…)` |

## Methods

- **`list[T]`**: `append`, `extend`, `insert`, `pop`, `remove`, `index`, `count`, `sort`, `reverse`, `clear`, `copy`, `contains`; and, since a list is a .NET `List<T>`, its methods too.
- **`dict[K, V]`**: see [Dictionaries](#dictionaries), below.
- **`set[T]`**: `add`, `remove`, `discard`, `pop`, `union`, `intersection`, `difference`, `issubset` and the others, and the operators `|`, `&`, `-`, `^`.
- **`str`**: see [Strings](#strings), below.

## Strings

A `str` is text. Strings can't be changed, so every method that seems to change one gives a new string, and the original stays as it was. Lengths, indices and widths count characters (Unicode code points), so `len("héllo")` is 5.

### Operators

| Operation | Gives | Example |
|---|---|---|
| `a + b` | the two joined | `"ab" + "cd"` is `"abcd"` |
| `s * n` | `s` repeated `n` times | `"ab" * 3` is `"ababab"` |
| `sub in s`, `sub not in s` | whether `s` contains `sub` | `"ell" in "hello"` is `True` |
| `s[i]` | the character at `i`, counting from 0; a negative `i` counts from the end | `"hello"[-1]` is `"o"` |
| `s[start:stop]` | the characters from `start` up to, not including, `stop` | `"hello"[1:3]` is `"el"` |
| `for c in s:` | each character in turn, as a `str` | |
| `==`, `<`, `>` and the others | comparison, character by character | `"apple" < "banana"` is `True` |

`len(s)` counts the characters, `str(x)` turns any value into text, and `ord(c)` and `chr(n)` convert between a character and its code point.

### Cleaning up

| Method | Gives |
|---|---|
| `s.strip()` | `s` without whitespace at either end |
| `s.strip(chars)` | `s` without any of the characters in `chars` at either end |
| `s.lstrip()`, `s.lstrip(chars)` | the same, at the start only |
| `s.rstrip()`, `s.rstrip(chars)` | the same, at the end only |
| `s.removeprefix(p)` | `s` without `p` at the start, if it's there |
| `s.removesuffix(p)` | `s` without `p` at the end, if it's there |
| `s.replace(old, new)` | `s` with every `old` replaced by `new` |

`strip(chars)` treats `chars` as a set of characters, not as one piece of text, so `strip(".,!")` removes any mix of those three from the ends.

```taipan
print("  Hello, World!  ".strip())
print("--==hi==--".strip("-="))
print("snake.".strip(".,;:!?"))
print("test_file.tpn".removeprefix("test_"))
print("a-b-c".replace("-", "+"))
```

```output
Hello, World!
hi
snake
file.tpn
a+b+c
```

### Splitting and joining

| Method | Gives |
|---|---|
| `s.split()` | a `list[str]` of the words in `s`, split on runs of spaces, tabs and newlines, with none empty |
| `s.split(sep)` | a `list[str]` of the parts between each `sep`; parts may be empty |
| `s.split(sep, maxsplit)` | the same, splitting at most `maxsplit` times, from the left |
| `s.rsplit(sep, maxsplit)` | the same, from the right |
| `s.splitlines()` | a `list[str]` of the lines in `s`, without their line endings |
| `sep.join(xs)` | the strings in `xs` joined, with `sep` between each |
| `s.partition(sep)` | a `tuple[str, str, str]`: the part before the first `sep`, `sep`, and the part after; `(s, "", "")` if there's no `sep` |
| `s.rpartition(sep)` | the same, at the last `sep`; `("", "", s)` if there's no `sep` |

An empty `sep` raises `ValueError`. `maxsplit` may be given by name, as `split(",", maxsplit=1)`.

```taipan
print("a b  c".split())
print("a,b,,c".split(","))
print("a,b,c".split(",", maxsplit=1))
print("one\ntwo".splitlines())
print("-".join(["a", "b", "c"]))
print("key=value".partition("="))
```

```output
["a", "b", "c"]
["a", "b", "", "c"]
["a", "b,c"]
["one", "two"]
a-b-c
("key", "=", "value")
```

### Searching

| Method | Gives |
|---|---|
| `s.startswith(p)`, `s.endswith(p)` | whether `s` starts or ends with `p` |
| `s.find(sub)` | the index of the first `sub` in `s`, or `-1` if there's none |
| `s.rfind(sub)` | the index of the last `sub`, or `-1` |
| `s.index(sub)` | the index of the first `sub`; raises `ValueError` if there's none |
| `s.count(sub)` | how many times `sub` appears, without overlaps |

To test whether `s` contains `sub` anywhere, write `sub in s`.

```taipan
print("hello".startswith("he"), "hello".endswith("x"))
print("banana".find("na"), "banana".rfind("na"), "banana".find("x"))
print("banana".count("a"))
```

```output
True False
2 4 -1
3
```

### Changing case

| Method | Gives |
|---|---|
| `s.lower()`, `s.upper()` | `s` in lowercase or uppercase |
| `s.title()` | each word starting with a capital, the rest lowercase |
| `s.capitalize()` | the first character a capital, the rest lowercase |
| `s.swapcase()` | lowercase made uppercase, and uppercase lowercase |

```taipan
print("Hello".lower(), "Hello".upper())
print("hello world".title(), "hello WORLD".capitalize())
```

```output
hello HELLO
Hello World Hello world
```

### Padding

| Method | Gives |
|---|---|
| `s.ljust(width)`, `s.rjust(width)`, `s.center(width)` | `s` padded with spaces to `width` characters, on the right, on the left, or on both sides |
| `s.ljust(width, fill)` and the others | the same, padded with the character `fill` |
| `s.zfill(width)` | `s` padded with zeros on the left, after any sign |

A string already `width` long or longer is unchanged. An [f-string](#formatting) can pad as it formats, which is often simpler.

```taipan
print("[" + "ab".ljust(5) + "]", "[" + "ab".rjust(5) + "]")
print("ab".center(6, "*"), "42".zfill(5), "-42".zfill(5))
```

```output
[ab   ] [   ab]
**ab** 00042 -0042
```

### Testing characters

Each gives a `bool`, and is `False` for an empty string.

| Method | `True` when |
|---|---|
| `s.isdigit()` | every character is a digit |
| `s.isalpha()` | every character is a letter |
| `s.isalnum()` | every character is a letter or a digit |
| `s.isspace()` | every character is whitespace |
| `s.isupper()`, `s.islower()` | it has letters, and they're all uppercase, or all lowercase |

To turn text into a number, use `int(s)` or `float(s)`. Each gives an optional, `None` when the text isn't a number, so you have to handle that case (see [Optionals](../../tour/optionals/)):

```taipan
let n = int("42") ?? 0
print(n + 1, int("forty") ?? -1)
```

```output
43 -1
```

### Formatting

An f-string puts values into text: `f"{name} is {age}"`. After a colon, a *format spec* says how to write the value, using Python's format mini-language:

| Spec | Means | Example | Gives |
|---|---|---|---|
| `<n`, `>n`, `^n` | at least `n` wide, aligned left, right or center | `f"[{'ab':>4}]"` | `[  ab]` |
| `.nf` | a `float` with `n` digits after the point | `f"{3.14159:.2f}"` | `3.14` |
| `,` | commas between thousands | `f"{1234567:,}"` | `1,234,567` |
| `0n` | padded with zeros to `n` wide | `f"{7:03}"` | `007` |
| `+` | always a sign | `f"{5:+}"` | `+5` |
| `%` | a `float` as a percentage | `f"{0.25:.0%}"` | `25%` |
| `x`, `b`, `o` | an `int` in hexadecimal, binary or octal | `f"{255:x}"` | `ff` |

Parts combine in the order fill, alignment, sign, `0`, width, `,`, precision, type, as in `f"{total:>10,.2f}"`. A fill character goes before the alignment, as in `f"{'ab':*^6}"`, which gives `**ab**`. Format specs apply to `int`, `float` and `str` values, and a spec that doesn't fit the value's type is a compile error. `f"{x = }"` writes the expression and its value, which helps when debugging.

```taipan
for word, count in [("the", 10), ("taipan", 6)]:
    print(f"{word:<10}{count:>3}")
print(f"{1234:,} words, {0.5:.0%} done")
```

```output
the        10
taipan      6
1,234 words, 50% done
```

## Dictionaries

A `dict[K, V]` maps keys of type `K` to values of type `V`, as `dict[str, int]` maps words to counts. Each key appears once, and the keys keep the order they were first added in.

```taipan
let ages = {"ada": 36, "alan": 41}
var counts: dict[str, int] = {}
```

The type is inferred from a literal with something in it, so `ages` is a `dict[str, int]`. An empty `{}` needs its type written. All the keys share one type, and so do all the values: for records with fields of different types, use a [dataclass](../../tour/classes/), and for JSON, `dict[str, py]`.

A dict declared with `let` can still be changed: `let` only means the name can't be given a different dict.

Keys must be `int`, `str` or `bool` values, tuples of those, or instances of plain classes. Lists and dicts can't be keys, since they can change.

### Operators

| Operation | Gives |
|---|---|
| `d[k]` | the value for `k`; raises `KeyError` if there's none |
| `d[k] = v` | sets the value for `k`, adding the key if it's new |
| `del d[k]` | removes `k`; raises `KeyError` if it's not there |
| `k in d`, `k not in d` | whether `d` has the key `k` |
| `len(d)` | the number of keys |
| `for k in d:` | each key in turn, in order |
| `d == other` | whether both have the same keys and values, in any order |

Adding or removing keys while looping over a dict raises `RuntimeError`. Changing the value of an existing key is fine.

### Methods

| Method | Gives |
|---|---|
| `d.get(k)` | the value for `k` as an optional, `None` if there's none |
| `d.get(k, default)` | the value for `k`, or `default` if there's none |
| `d.keys()`, `d.values()` | a new list of the keys, or of the values, in order |
| `d.items()` | a new `list[tuple[K, V]]` of the keys and values, in order |
| `d.pop(k)` | removes `k` and gives its value; raises `KeyError` if it's not there |
| `d.pop(k, default)` | the same, but gives `default` if `k` isn't there |
| `d.remove(k)` | removes `k` if it's there, and gives whether it was; Taipan's own, not Python's |
| `d.setdefault(k, default)` | the value for `k`, first adding `k` with `default` if it isn't there |
| `d.update(other)` | adds or replaces every key and value from `other` |
| `d.copy()` | a new dict with the same keys and values |
| `d.clear()` | removes every key |

`get(k)` with no default gives an optional (`V?`), which has to be checked before it's used. Give a default, or use `is some` (see [Optionals](../../tour/optionals/)):

```taipan
let ages = {"ada": 36}
print(ages.get("ada", 0) + 1)
if ages.get("bob") is some age:
    print(age)
else:
    print("no bob")
```

```output
37
no bob
```

### Counting and grouping

`get` with a default counts things, and `setdefault` groups them:

```taipan
var counts: dict[str, int] = {}
for word in ["the", "taipan", "the"]:
    counts[word] = counts.get(word, 0) + 1
print(counts)

var by_letter: dict[str, list[str]] = {}
for word in ["ant", "bee", "asp"]:
    by_letter.setdefault(word[0], []).append(word)
print(by_letter)
```

```output
{"the": 2, "taipan": 1}
{"a": ["ant", "asp"], "b": ["bee"]}
```

### Looping

`items()` gives each key with its value, and `sorted` puts them in another order. Its `key` says what to sort by: here the count, from highest, then the word:

```taipan
let counts = {"in": 4, "the": 10, "and": 4}
for word, n in counts.items():
    print(word, n)
for word, n in sorted(counts.items(), key=lambda p: (-p[1], p[0])):
    print(word, n)
```

```output
in 4
the 10
and 4
the 10
and 4
in 4
```

### Making dicts

| Form | Gives |
|---|---|
| `{"a": 1, "b": 2}` | a dict of those keys and values |
| `{k: f(k) for k in xs}` | a dict comprehension: one key and value for each `k` |
| `dict(zip(keys, values))` | a dict from two lists side by side |
| `dict(a=1, b=2)` | a `dict[str, …]` from names and values |
| `dict(d)` | a copy of `d` |

```taipan
print({n: n * n for n in range(4)})
print(dict(zip(["x", "y"], [1, 2])))
```

```output
{0: 0, 1: 1, 2: 4, 3: 9}
{"x": 1, "y": 2}
```

## Modules

| Module | Names |
|---|---|
| `sys` | `argv`, `exit`, `stdin`, `stdout`, `stderr` |
| `os` | `environ`, `getenv` |
| `types` | `TracebackType` |
| `asyncio` | `run`, `gather`, `sleep`, `wait_for`, `timeout` |

## Types

`int`, `float`, `bool`, `str`, `list[T]`, `dict[K, V]`, `set[T]`, `tuple[A, B, …]`, `tuple[T, ...]`, `T?`, `Callable[[A, B], R]`, `Iterable[T]`, `Iterator[T]`, `Generator[Y, S, R]`, `AsyncIterable[T]`, `AsyncIterator[T]`, `AsyncGenerator[Y, S]`, `Awaitable[T]`, `TextIO`, `object`, `py`, and the protocols `Comparable` and `Protocol`.

## Exception classes

As in Python: `BaseException`, `SystemExit`, `GeneratorExit`, `Exception`, `ArithmeticError`, `ZeroDivisionError`, `OverflowError`, `LookupError`, `IndexError`, `KeyError`, `ValueError`, `UnicodeError` (with `UnicodeDecodeError` and `UnicodeEncodeError`), `TypeError`, `RuntimeError`, `AssertionError`, `StopIteration`, `StopAsyncIteration`, `EOFError`, `OSError` with `FileNotFoundError`, `FileExistsError`, `PermissionError`, `IsADirectoryError` and `TimeoutError`, and `asyncio.CancelledError`; and Taipan's `PythonError` and `DotNetError`, for exceptions from Python and .NET.
