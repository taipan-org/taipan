---
title: Taipan for Python developers
description: What's the same as Python, what's stricter, and what's different.
sidebar:
  order: 4
---

Taipan is written in Python's syntax, so most Python code reads the same. The differences come from one decision: **types are checked when you compile, not when a line runs**. This page lists what that changes. [Differences from Python](../../reference/python-differences/) has the complete list.

## What stays the same

Indentation, `def`, `class`, `@dataclass`, `for` and `while`, `if`/`elif`/`else`, `try`/`except`/`finally`, `with`, `lambda`, comprehensions, f-strings with format specs, slicing, `*args` and `**kwargs`, generators with `yield`, `async`/`await`, chained comparisons such as `0 < x <= 10`, and the built-ins you know: `print`, `len`, `range`, `enumerate`, `zip`, `sorted`, `reversed`, `min`, `max`, `sum`, `any`, `all`, `map`, `filter`, `dict`, `ord`, `chr`, `isinstance`, `open`.

```taipan
@dataclass
class Item:
    name: str
    price: float
    count: int

def total(items: list[Item]) -> float:
    return sum(i.price * i.count for i in items)

let cart = [Item("tea", 4.5, 2), Item("cake", 3.0, 1)]
for i, item in enumerate(cart, start=1):
    print(f"{i}. {item.name:<6}{item.price * item.count:>6.2f}")
print(f"total {total(cart):.2f}")
```

```output
1. tea     9.00
2. cake    3.00
total 12.00
```

`@dataclass`, `@property`, `@override` and the other decorators are built in, so they need no import.

## Variables are declared

A name is declared with `let` (it can't be reassigned) or `var` (it can). Plain assignment only changes an existing variable, so a typo can't silently create a new one:

```taipan
var count = 0
for word in ["a", "b"]:
    cuont = count + 1
```

```error
main.tpn:3:5: error TP0201: undefined name 'cuont'
  help: if it's a new variable, declare it with 'let', 'var' or 'const'
```

`const` declares a top-level constant. Blocks have their own scope, as in C#, and a name in an inner block can't shadow one in an outer block.

## Functions read top-level variables

As in Python, a function reads the top-level `let` and `var` bindings of its file, and sees a `var`'s current value:

```taipan
let LIMIT = 3
var calls = 0

def check(n: int) -> bool:
    return n < LIMIT

def count() -> int:
    return calls

print(check(2), count())
calls = 5
print(count())
```

```output
True 0
5
```

There's no `global`, so a function can't assign to one, though it may change what one holds, as with `table[k] = v`. Calling a function at the top level before a binding it reads is set is a compile error, where Python would raise `NameError` when the program runs:

```taipan
def check(n: int) -> bool:
    return n < LIMIT

print(check(2))
let LIMIT = 3
```

```error
main.tpn:4:7: error TP0201: 'check' reads 'LIMIT', which isn't set until line 5
  help: move this below line 5
```

## Annotate functions; the rest is inferred

Parameters and results are annotated, as in typed Python. Locals take their type from their initializer. An empty `[]` or `{}` needs a type from somewhere, such as `let names: list[str] = []`.

## No `None` surprises

A value that might be `None` has an optional type, written `T?` (Python's `T | None`). It can't be used as a `T` until you've checked it:

```taipan
let ages = {"ada": 36}
let age = ages.get("bob")
print(age + 1)
```

```error
main.tpn:3:7: error TP0203: int? must be narrowed before use
  help: narrow it with 'is some', or give a default with '??'
```

Check it with `is not None`, bind it with `is some`, or give a default with `??`:

```taipan
let ages = {"ada": 36}
if ages.get("ada") is some age:
    print(age + 1)
print(ages.get("bob") ?? 0)
```

```output
37
0
```

## No truthiness

Conditions must be `bool`. Write what you mean: `if xs:` is `if len(xs) > 0:`, and `if name:` is `if name != "":` or `if name is not None:`.

```taipan
let xs: list[int] = []
if xs:
    print("not empty")
```

```error
main.tpn:2:4: error TP0205: condition must be bool, found list[int]
```

## Enums with data, and exhaustive `match`

`enum` declares a sum type, whose variants may carry fields. A `match` must handle every variant, so adding one shows you every place to update. There's no `case` keyword: each arm is a pattern and a colon.

```taipan
enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)

def area(s: Shape) -> float:
    match s:
        Circle(r): return 3.14159 * r * r
        Rect(w, h): return w * h

print(area(Rect(2.0, 3.0)))
```

```output
6.0
```

## Numbers

- `int` is 64-bit. Overflow raises `OverflowError` instead of growing.
- `/` gives a `float` and `//` an `int`, as in Python. An `int` widens to a `float` where one is expected, and nothing else converts implicitly: use `int(x)`, `float(x)` and `str(x)`.
- `int("42")` and `float("2.5")` give an optional, `None` when the text isn't a number, rather than raising.
- `int ** int` is an `int`, so `2 ** -1` raises `ValueError` rather than giving `0.5`. Write `2.0 ** -1` for a float.

## Smaller differences

- **Defaults are evaluated on every call**, so `def f(xs: list[int] = [])` gets a new list each time. Python's shared-default trap doesn't exist.
- **A nested function needs `nonlocal` to assign to an outer variable**, as in Python, but a new local of the same name isn't possible, so forgetting it is a compile error (`TP0236`). Top-level variables have no `global`: a nested function can read them but not assign to them.
- **`*=` isn't supported on a list, nor `-=`, `|=`, `&=` or `^=` on a set**: write `xs = xs * n` or `s = s | t`, which make a new collection. `[0] * n` and `"-" * n` work as in Python.
- **A hole in a triple-quoted f-string stays on one line**, where Python 3.12 allows several. `\N{…}` escapes and `bytes` aren't supported. String prefixes are lowercase.
- **A main file can't be imported**, and there's no `__name__`. To test a command-line tool, put its code in a module that the main file and the test file both import.
- **A Python exception is a `PythonError`**: `except KeyError` catches Taipan's `KeyError`, not one from Python code. Catch Python's with `from python import builtins` and `except builtins.KeyError`, or catch `PythonError`.
- **Each loop iteration has its own variable**, so `[lambda: i for i in range(3)]` gives functions returning 0, 1 and 2.
- **No conditional expression**: `a if c else b` is written as an `if` statement or a `match` expression.
- **Strings in containers print with double quotes**: `print(["a"])` is `["a"]`.
- **An `int` that widens to a `float` is a `float`**: `let x: float = 1` prints `1.0`, and `[1, 2.5]` prints `[1.0, 2.5]`.
- **Calling an async function starts it**, as a .NET task starts, rather than waiting to be awaited.
- **`keys()`, `values()` and `items()` give new lists**, not live views.
- **Some exception messages are Taipan's own**, so `str(e)` can differ from Python's for rarer errors.
- **Infinite recursion ends the process**, as in .NET, and can't be caught, so there's no `RecursionError`. Recursion can go much deeper than Python's limit of 1,000.
- **Lists are invariant**: a `list[Dog]` isn't a `list[Animal]`, since code holding it as one could add a `Cat`.
- **Imports are of Taipan modules**, files beside the main file. Python modules are imported with `from python import json` and .NET namespaces with `from dotnet.System.IO import File`.
- **No dynamic features**: no `eval`, `setattr`, monkey-patching or multiple inheritance. Protocols cover what multiple inheritance usually does.

## Using Python libraries

Python packages are still there, through `from python import …`. Their values have the type `py`, which Python checks at run time, and you convert them back with `as`:

```taipan
from python import json

let data = json.loads("{\"ok\": true, \"items\": [1, 2, 3]}")
let items = data["items"] as list[int]
print(sum(items))
```

```output
6
```

See [Calling Python](../../guides/python/).
