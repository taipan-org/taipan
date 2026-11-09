---
title: Strings and f-strings
description: "String literals, quotes, escape sequences, raw and triple-quoted strings, and f-strings with conversions, = and the format-spec mini-language."
sidebar:
  order: 10
---

A `str` is a piece of text: a sequence of Unicode characters (code points), which can't be changed once it's made. This page covers how strings are written in a program, and how f-strings put values into them. What you can do with a string once you have one, its operators and methods, is on the [str reference](../../library/str/).

String literals and f-strings are defined in [§1.3](../../reference/specification/#13-tokens), and how f-strings format their values in [§6](../../reference/specification/#6-built-ins-v01).

## String literals

A string literal is text between quotes. Double quotes `"…"` and single quotes `'…'` mean exactly the same thing, so choose whichever saves escaping a quote inside:

```taipan
print("double", 'single', "it's", 'say "hi"')
```

```output
double single it's say "hi"
```

A string in `"` or `'` quotes must end on the line it starts on. One that doesn't is error `TP0003`:

```taipan
print("abc)
```

```error
main.tpn:1:7: error TP0003: unterminated string
```

When a string is displayed by itself, with `print` or `str()`, it's written as it is. Inside a list, dict or other container, or with `repr()`, it's written as a literal, always in double quotes, with special characters escaped:

```taipan
let word = "it's"
print(word, [word], repr("a\tb"))
```

```output
it's ["it's"] "a\tb"
```

Two literals next to each other aren't joined into one: join them with `+`.

```taipan
print("ab" "cd")
```

```error
main.tpn:1:12: error TP0100: expected ')', found string
```

## Escape sequences

Inside a string, a backslash `\` starts an *escape sequence*, which stands for a character that's hard to type or would end the string:

| Escape | Character |
|---|---|
| `\n` | newline |
| `\t` | tab |
| `\r` | carriage return |
| `\\` | a backslash |
| `\"`, `\'` | a quote |
| `\a`, `\b`, `\f`, `\v` | bell, backspace, form feed, vertical tab |
| `\0`, `\012` | the character with that octal (base 8) code, up to three digits |
| `\xhh` | the character with the hex code `hh`, exactly two hex digits |
| `\uhhhh` | the character with the hex code `hhhh`, exactly four hex digits |
| `\Uhhhhhhhh` | the character with the hex code `hhhhhhhh`, exactly eight hex digits |
| `\` at the end of a line | nothing: the string continues on the next line, without a newline |

```taipan
print("tab:\tend", "back\\slash", "quote\"", 'apos\'')
print("line one\nline two")
print("\x41\u00e9\U0001F40D", len("\U0001F40D"))
print("one \
two")
```

```output
tab:	end back\slash quote" apos'
line one
line two
Aé🐍 1
one two
```

Any other character after a backslash is error `TP0005`. So is a `\x`, `\u` or `\U` without enough hex digits, and one naming a code that isn't a character a string can hold: above `U+10FFFF`, or a *surrogate* (`U+D800` to `U+DFFF`), which is only half of a character in some encodings. Named characters (`\N{…}`) aren't supported. To write a backslash itself, write `\\`, or use a [raw string](#raw-strings).

```taipan
print("C:\data")
```

```error
main.tpn:1:10: error TP0005: invalid escape sequence '\d'
```

```taipan
print("\x4")
```

```error
main.tpn:1:8: error TP0005: '\x' needs 2 hex digits
```

```taipan
print("\ud83d")
```

```error
main.tpn:1:8: error TP0005: '\ud83d' isn't a character a string can hold
```

## Triple-quoted strings

A string between three double quotes `"""…"""`, or three single quotes `'''…'''`, may span several lines. Its newlines and indentation are kept, exactly as written. Quotes inside it don't need escaping, unless three in a row would end it. Escape sequences work as in any string, so a `\` at the end of a line leaves that newline out.

```taipan
let poem = """roses are red,
    violets are blue"""
print(poem)
let text = """\
no newline at the start
"""
print(repr(text))
```

```output
roses are red,
    violets are blue
"no newline at the start\n"
```

A string literal on a line by itself, often a triple-quoted one, is a statement that does nothing. It's used as a *docstring*, describing the function, class or file it begins.

## Raw strings

An `r` immediately before the opening quote makes a *raw string*, in which a backslash is an ordinary character: `r"\d+"` is the four characters `\`, `d`, `+`. Raw strings are useful for text with many backslashes, such as Windows paths and patterns. Any of the quote forms may be raw, including `r"""…"""`.

A backslash still keeps a quote after it from ending the string, but stays in the string with it, so `r"\""` is a backslash followed by a quote. For the same reason, a raw string can't end with an odd number of backslashes: `r"\"` is unterminated.

```taipan
print(r"\d+\.\w", r"C:\new", r'\'')
```

```output
\d+\.\w C:\new \'
```

```taipan
print(r"\")
```

```error
main.tpn:1:8: error TP0003: unterminated string
```

The prefix is a lowercase `r`. Other prefixes, and an uppercase `R`, aren't supported:

```taipan
print(b"bytes")
```

```error
main.tpn:1:8: error TP0100: expected ')', found string
```

## f-strings

An *f-string* has an `f` immediately before its opening quote. Inside it, an expression in braces, called a *hole*, is computed when the f-string is, and replaced with its value written as text:

```taipan
let name = "api"
let count = 42
print(f"{name} has {count} items, {count * 2} after doubling")
```

```output
api has 42 items, 84 after doubling
```

An f-string may use either quote, may be triple-quoted, and may be raw as well, with the prefix `rf` or `fr`. Its literal text uses the same escape sequences as other strings. To write a brace itself, double it: `{{` and `}}`.

```taipan
let n = 3
print(f'{n} {{in braces}}')
print(rf"\d{n}", fr"{n}\n")
print(f"""first line
second, with {n}""")
```

```output
3 {in braces}
\d3 3\n
first line
second, with 3
```

### Holes

A hole holds one expression of any kind, except another f-string: names, arithmetic, calls, indexing, even a [`match` expression](../match/#match-expressions). A string inside a hole may use either kind of quote, including the f-string's own. A hole must stay on one line.

```taipan
let ages = {"ana": 31}
let names = ["ana", "ben"]
print(f"{ages["ana"]} {len(names)} {names[0].upper()} {", ".join(names)}")
```

```output
31 2 ANA ana, ben
```

A value is written as `print` would write it, its *display form*: numbers in decimal, `True` and `False`, `None`, a string as it is, and a list with its elements, strings in quotes. A class can define its own display form with `__str__` (see [Classes](../classes/)).

A `lambda` in a hole must be in parentheses, since its `:` would otherwise start a [format spec](#format-specs).

Mistakes in a hole are error `TP0006`:

```taipan
print(f"{}")
```

```error
main.tpn:1:9: error TP0006: empty f-string hole
```

```taipan
let n = 1
print(f"{f"{n}"}")
```

```error
main.tpn:2:10: error TP0006: f-strings cannot be nested
```

```taipan
print(f"total } 3")
```

```error
main.tpn:1:15: error TP0006: single '}' is not allowed in an f-string
```

### The parts of a hole

After the expression, a hole may have up to three more parts, in this order:

| Part | Written | Does |
|---|---|---|
| `=` | `{x=}`, `{x = }` | writes the expression's text, spaces included, before its value |
| conversion | `{x!r}`, `{x!s}` | `!r` writes the value as a literal, as inside a list; `!s` writes the display form, which is the default |
| format spec | `{x:>8.2f}` | a colon followed by a spec, which says how to lay the value out (below) |

So `{x}`, `{x=}`, `{x!r}`, `{x:>8}` and `{x=!r:>10}` are all holes. A `:` or `!` inside brackets, inside a string, or as part of `!=`, `==`, `<=` or `>=` belongs to the expression, so `{a != b}` and `{xs[1:3]}` work as expected.

With `=`, the value is written as `!r` would write it, unless the hole has its own conversion or format spec. That makes `=` useful when debugging:

```taipan
let name = "api"
let pi = 3.14159
print(f"{name=}", f"{pi = }", f"{pi=:.2f}", f"{name=!s}")
print(f"{name!r} {name!s} {name!r:>8}|")
```

```output
name="api" pi = 3.14159 pi=3.14 name=api
"api" api    "api"|
```

After a conversion, the value is a string, so a format spec then applies to it as to a string, as `{name!r:>8}` shows.

The only conversions are `!r` and `!s`:

```taipan
let name = "api"
print(f"{name!a}")
```

```error
main.tpn:2:14: error TP0006: invalid conversion '!a'
  help: use '!r' or '!s'
```

## Format specs

A *format spec*, after a colon at the end of a hole, controls how an `int`, `float` or `str` is written: its width and alignment, its sign, how many digits, and in which notation. The spec is made of these parts, each optional, in this order:

```text
[[fill]align][sign][z][#][0][width][grouping][.precision][type]
```

| Part | Written | Means |
|---|---|---|
| *fill* | any character, just before *align* | the character to pad with, instead of a space |
| *align* | `<`, `>`, `^` | pad to the left, right or centre of *width*; text aligns left by default, numbers right |
| | `=` | for numbers: pad between the sign and the digits |
| *sign* | `+` | always write a sign, `+` or `-` |
| | `-` | write a sign only for negative numbers, the default |
| | a space | a space for a positive number, `-` for a negative one |
| `z` | `z` | for floats: write `-0.0`, and values that round to it, as `0.0` |
| `#` | `#` | write `0b`, `0o` or `0x` before a binary, octal or hex number; keep a float's decimal point even with no digits after it |
| `0` | `0` | pad with zeros after the sign, as `=` alignment with a fill of `0` |
| *width* | a number | the least number of characters to write; a longer value isn't cut |
| *grouping* | `,` or `_` | separate thousands with commas or underscores; with `_` and `b`, `o`, `x` or `X`, every four digits |
| *precision* | `.` and a number | for floats, how many digits (below); for strings, the most characters to write |
| *type* | a letter or `%` | the notation (below) |

The width and alignment are often all you need, for lining text up in columns:

```taipan
let rows = [("apples", 3, 1.5), ("kiwis", 12, 0.25)]
for name, count, price in rows:
    print(f"{name:<8}|{count:>4}|{price:>7.2f}|")
```

```output
apples  |   3|   1.50|
kiwis   |  12|   0.25|
```

### Types for an `int`

| Type | Writes | `255` gives |
|---|---|---|
| `d`, or none | decimal | `255` |
| `b` | binary | `11111111` |
| `o` | octal | `377` |
| `x`, `X` | hexadecimal, in lowercase or capitals | `ff`, `FF` |
| `c` | the character with that code point | `ÿ` |
| `e`, `f`, `g`, `%` and the other float types | the number as a `float` | |

A precision isn't allowed on an `int` with these types: use a float type, as `{n:.2f}`, to write digits after the point.

```taipan
let n = 255
print(f"{n:b} {n:o} {n:x} {n:X} {n:#x} {n:#b} {65:c} {n:.1f}")
print(f"[{n:<6}] [{n:>6}] [{n:^6}] [{n:*^7}] [{n:06}] [{-42:=6}]")
print(f"{n:+} {-n:+} [{n: }] {1234567:,} {1234567:_} {n:#010b} {1048575:_x}")
```

```output
11111111 377 ff FF 0xff 0b11111111 A 255.0
[255   ] [   255] [ 255  ] [**255**] [000255] [-   42]
+255 -255 [ 255] 1,234,567 1_234_567 0b11111111 f_ffff
```

### Types for a `float`

| Type | Writes | Default precision |
|---|---|---|
| `f`, `F` | fixed-point: *precision* digits after the point; `F` writes `INF` and `NAN` in capitals | 6 |
| `e`, `E` | scientific notation, `1.23e+03`: *precision* digits after the point; `E` writes a capital `E` | 6 |
| `g`, `G` | *precision* significant digits, in fixed-point or scientific notation, whichever suits the number's size, without trailing zeros | 6 |
| `%` | the number times 100, in fixed-point, followed by `%` | 6 |
| none | as `g`, but with at least one digit after the point, and as many digits as needed to give the exact value back when there's no precision | |

```taipan
let x = 1234.5678
print(f"{x:f} {x:.2f} {x:e} {x:.2E} {x:g} {x:.3g} {x:,.2f}")
print(f"{0.25:%} {0.25:.1%} {2.0} {2.0:g} {x:10.1f}| {x:+.1f} {x:010.2f}")
print(f"{-0.0:.1f} {-0.0:z.1f} {-0.001:z.2f} {3.0:.0f} {3.0:#.0f}")
```

```output
1234.567800 1234.57 1.234568e+03 1.23E+03 1234.57 1.23e+03 1,234.57
25.000000% 25.0% 2.0 2     1234.6| +1234.6 0001234.57
-0.0 0.0 0.00 3 3.
```

### Types for a `str`

A string's type is `s`, or none. A precision cuts the string to that many characters. `=` alignment, a sign, `z`, `#`, `0` and grouping are for numbers, and don't apply to strings.

```taipan
let s = "taipan"
print(f"[{s:<10}] [{s:>10}] [{s:^10}] [{s:-^10}] [{s:.3}] [{s:>6.2}]")
```

```output
[taipan    ] [    taipan] [  taipan  ] [--taipan--] [tai] [    ta]
```

### Specs are checked when compiling

The compiler checks each spec against the type of its hole, so a mistake is a compile error at the spec, rather than a failure when the line runs. A spec that isn't in the mini-language at all is error `TP0006`. A spec whose parts don't apply to the hole's type is error `TP0200`, with help naming the part:

```taipan
let price = "3.50"
print(f"{price:.2f}")
```

```error
main.tpn:2:16: error TP0200: format spec '.2f' isn't valid for str
  help: 'f' formats a number, not str
```

```taipan
let n = 3
print(f"{n:.3d}")
```

```error
main.tpn:2:12: error TP0200: format spec '.3d' isn't valid for int
  help: precision isn't allowed for an int formatted with 'd'
```

```taipan
let n = 3
print(f"{n:Q}")
```

```error
main.tpn:2:12: error TP0006: invalid format spec 'Q'
```

Format specs apply only to `int`, `float` and `str` values. Any other type, including a `bool` and an optional, is an error: convert the value with `str()` first, or narrow the optional (see [Optionals](../optionals/)).

```taipan
let ready = True
print(f"{ready:>6}")
```

```error
main.tpn:2:16: error TP0200: format specs apply to int, float and str, found bool
  help: convert it with str() or int() first
```

```taipan
let maybe: int? = 3
print(f"{maybe:>5}")
```

```error
main.tpn:2:16: error TP0200: format specs apply to int, float and str, found int?
  help: narrow the optional first
```

What isn't supported:
- **Nested specs**, such as `{x:{width}}`, where part of the spec comes from another hole, are error `TP0006`. Pad with a string method instead, as `str(x).rjust(width)`.
- **The `n` type**, which formats by the computer's language settings, is error `TP0006`. Use `d`, `f` or `g`, with `,` or `_` for grouping.

```taipan
let width = 6
print(f"{42:{width}}")
```

```error
main.tpn:2:13: error TP0006: format specs can't be nested
```
