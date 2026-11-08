---
title: Differences from Python
description: What Taipan adds, what it rejects and what to write instead, where it behaves differently, and how Python code meets Taipan code.
sidebar:
  order: 5
---

Taipan reads like Python, and code that compiles does what a Python developer expects, with the few exceptions listed below. When Taipan can't do what the Python would, the code doesn't compile, and the error says what to write instead. This page collects those rules in one place. [Taipan for Python developers](../../start/python-developers/) is the gentler introduction.

## What Taipan adds

These are Taipan's own, so they can't be mistaken for Python:

| Taipan | What it does |
|---|---|
| `let x = 1`, `var y = 2` | declares a name, which can't or can be reassigned |
| `const LIMIT = 10` | a top-level constant, visible throughout its file and importable from it |
| `T?`, `x ?? default`, `if x is some v:` | optional values, which must be checked before use |
| `enum Shape:` with variants that carry fields | sum types |
| `match x:` with patterns and no `case` | exhaustive matching, checked at compile time |
| `from python import json` | Python modules, whose values have the type `py` |
| `from dotnet.System.IO import File` | .NET types, checked at compile time |

## Python that doesn't compile, and what to write

| Python | Taipan |
|---|---|
| `a if c else b` | a `match` expression, or an `if` statement |
| `x = 1` for a new name | `let x = 1` or `var x = 1` |
| `a = 1; b = 2` | one statement per line |
| `if xs:`, `while n:` | `if len(xs) > 0:`, `while n != 0:`, since conditions must be `bool` |
| `{"id": 1, "title": "x"}` | a dataclass, or `dict[str, py]` for JSON |
| `global x` | pass the value in and return the new one; functions read top-level names but can't assign them |
| `if __name__ == "__main__":` | nothing: a main file can't be imported, so shared code goes in a module |
| `for …: … else:` | a flag, or a `return` from a function |
| `xs *= 2` on a list | `xs = xs * 2` |
| `"{}".format(x)`, `"%d" % x` | an f-string |
| `x in xs in ys`, `a is b is c` | two comparisons joined by `and` |
| `except RecursionError:` | nothing: infinite recursion ends the process, as in .NET |
| `"\N{EM DASH}"`, `b"bytes"`, `R"raw"` | `"—"`, a `str`, and `r"raw"` |
| `eval`, `setattr`, multiple inheritance | nothing: protocols cover what multiple inheritance usually does |

## Where Taipan behaves differently

These compile, and they're the only places where Taipan quietly does something other than what Python would. Each has a reason, in the [specification's list](../specification/):

- **Defaults, field defaults and loop variables are new each time.** `def f(xs: list[int] = [])` gets a new list on each call, and `[lambda: i for i in range(3)]` gives functions returning 0, 1 and 2.
- **An `int` that widens to a `float` is a `float`.** `let x: float = 1` prints `1.0`.
- **`int(s)` and `float(s)` give an optional**, `None` when the text isn't a number, rather than raising.
- **Calling an async function starts it**, as a .NET task starts.
- **`keys()`, `values()` and `items()` give new lists**, not live views.
- **Adding to or removing from a list during a loop over it raises `RuntimeError`.** Writing to its elements is fine.
- **Exception messages are Taipan's own.**
- **Comparing an `int` with a `float` converts the `int`**, so very large numbers can compare equal.
- **A string can't hold a lone surrogate.**
- **Case conversion and number parsing follow .NET**: `"ß".upper()` is `ß`, and only ASCII digits parse.
- **`isinstance(v, int)` on a Python `bool` is `False`.**
- **A plain object prints as `<C object>`**, without a memory address.
- **Strings print with double quotes** inside lists, dicts and `repr`.

## Where Python meets Taipan

Python libraries are there through `from python import …`. Their values have the type `py`, which Python checks when the program runs. Everything else is checked when it compiles.

- **Converting:** `value as int`, `value as list[str]` and the like turn a `py` value into a Taipan one, raising `TypeError` if it doesn't convert. A Python `bool` isn't an `int` here.
- **Passing:** Taipan values go to Python as Python's equivalents. A list or dict literal passed to Python may mix types, as `json.dumps({"id": 1, "title": "x"})` does.
- **Exceptions:** a Python exception arrives as Taipan's `PythonError`, whose `type` is the Python class's name, `message` its text and `value` the exception itself. Taipan's own built-in classes, such as `KeyError`, are separate, so `except KeyError` doesn't catch a `KeyError` raised by Python code. The compiler warns about that (`TP0306`). Catch Python's by its class:

```taipan
from python import json
from python import builtins

let data = json.loads("{\"a\": 1}")
try:
    print(data["status"])
except builtins.KeyError as e:
    print("missing:", e.message)
```

```output
missing: 'status'
```

`except json.JSONDecodeError` and other library classes work the same way, and `except builtins.ValueError` catches its subclasses too, as in Python. `except PythonError` catches any of them. An uncaught one prints Python's last line, followed by a note that it came from Python.
