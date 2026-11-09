---
title: str
description: "Strings: operators, every method, converting to numbers, and format specs."
sidebar:
  order: 3
---

A `str` is text. Strings can't be changed, so every method that seems to change one gives a new string, and the original stays as it was. Lengths, indices and widths count characters (Unicode code points), so `len("héllo")` is 5.

## Operators

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

## Cleaning up

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

## Splitting and joining

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

## Searching

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

## Changing case

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

## Padding

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

## Testing characters

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

## Formatting

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
