---
title: with
description: "The with statement: files, several context managers at once, writing your own with __enter__ and __exit__, and when an exception is suppressed."
sidebar:
  order: 14
---

`with` runs a block of code between a setup step and a cleanup step, and makes sure the cleanup happens however the block ends: normally, through `return`, `break` or `continue`, or because of an exception. The object that does the setup and cleanup is a *context manager*. The most common one is an open file, which `with` closes at the end of the block.

`with` is defined in [§3.17](../../reference/specification/#317-context-managers-and-with-m37). For `async with`, see [async and await](../async/#async-with).

## Files

`open` gives a file, and a file is a context manager: `with open(path) as f:` names the file `f` for the block, and closes it when the block ends.

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-with-notes.txt"
with open(path, "w") as f:
    f.write("first\nsecond\n")
with open(path) as f:
    for line in f:
        print(line.strip())
```

```output
first
second
```

## The form

```
with EXPRESSION as NAME:
    BLOCK
```

`as NAME` is optional: `with lock:` runs the setup and cleanup without naming anything. The name is a `let` that exists only inside the block, as a `for` loop's variable does, so it can't be used after the block:

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-with-scope.txt"
with open(path, "w") as f:
    f.write("x")
print(f.closed)
```

```error
main.tpn:6:7: error TP0201: undefined name 'f'
```

The name must be a plain name; unpacking into several names is not supported. Since `as` at the top of a `with` item names the variable, converting a value with `as` there needs parentheses: `with (x as Lock) as held:`.

## Several managers

Several items separated by commas are nested, left to right: `with a as x, b as y:` means `with a as x:` with `with b as y:` inside it. So they're set up in order, and cleaned up in the opposite order.

```taipan
import os

let tmp = os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp"
let source = tmp + "/taipan-with-source.txt"
let target = tmp + "/taipan-with-target.txt"
with open(source, "w") as f:
    f.write("one\ntwo\n")

with open(source) as src, open(target, "w") as dst:
    for line in src:
        dst.write(line.upper())

with open(target) as f:
    print(f.read(), end="")
```

```output
ONE
TWO
```

Wrapping the item list in parentheses, `with (a as x, b as y):`, is not supported yet.

## Writing a context manager

Any class with both of these methods, its own or inherited, is a context manager:

- **`__enter__(self) -> T`** runs before the block. What it returns is what `as` names, and its return type is the name's type. It often returns `self`.
- **`__exit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> bool`** runs after the block. When the block ended without an exception, all three arguments are `None`. When it raised one, they are the exception's class, the exception, and its traceback. Returning `True` *suppresses* the exception: it stops there, and the program carries on after the `with`. Returning `False` lets it carry on outward. An `__exit__` that never suppresses can be declared `-> None` instead.

`TracebackType` comes from the built-in module `types`.

```taipan
from types import TracebackType

class Step:
    name: str

    def __init__(self, name: str):
        self.name = name

    def __enter__(self) -> Step:
        print("start", self.name)
        return self

    def __exit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> bool:
        if exc_type is some t:
            print("stop", self.name, "after", t.__name__, exc)
        else:
            print("stop", self.name)
        return False

with Step("build") as s:
    print("working on", s.name)

try:
    with Step("outer"), Step("inner"):
        raise ValueError("broken")
except ValueError as e:
    print("caught", e)
```

```output
start build
working on build
stop build
start outer
start inner
stop inner after ValueError broken
stop outer after ValueError broken
caught broken
```

A value that isn't a context manager is `TP0200`, with help saying what to add. An optional one has to be narrowed first (`TP0203`):

```taipan
with 42 as n:
    print(n)
```

```error
main.tpn:1:6: error TP0200: int is not a context manager
  help: define '__enter__' and '__exit__'
```

## What runs when

1. The expression is evaluated, then its `__enter__` is called, and the result is given the name after `as`. If either raises, `__exit__` isn't called: the setup never finished.
2. The block runs.
3. If the block ends normally, or through `return`, `break` or `continue`, `__exit__(None, None, None)` is called. A `return`'s value is evaluated before `__exit__` runs.
4. If the block raises an exception, of any class, `SystemExit` included, `__exit__` is called with it. If `__exit__` returns `True`, the exception is suppressed and the program continues after the `with`. Otherwise the exception carries on, unchanged, and its traceback still points at the line where it was raised.
5. If `__exit__` itself raises, that exception replaces the one in flight, as one raised in a `finally` block does.

With several items, a later item is inside the earlier ones, so if the second item's `__enter__` raises, the first item's `__exit__` is still called.

```taipan
from types import TracebackType

class Quiet:
    def __enter__(self) -> Quiet:
        return self

    def __exit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> bool:
        if exc is some e:
            print("suppressed:", e)
        return True

with Quiet():
    print("before")
    print([1, 2][5])
    print("not reached")
print("after the with")
```

```output
before
suppressed: list index out of range
after the with
```

A `type[C]` value, the type of `exc_type`, is a class used as a value. Its one attribute is `__name__`, and it displays as `<class 'ValueError'>` for a built-in class and `<class '__main__.Step'>` for one defined in the main file.

## Suppression and the compiler's checks

Whether an `__exit__` can suppress an exception changes what the compiler knows about the code after the `with`:

- **`-> bool`** may suppress, so the `with` may end after any statement of its block, as a `try` with an `except` clause may. A block that always returns doesn't make the `with` always return, and a variable first assigned inside the block isn't known to be assigned after it.
- **`-> None`** never suppresses, so the `with` is checked as its block alone.
- A file's `__exit__` never suppresses.

```taipan
from types import TracebackType

class Quiet:
    def __enter__(self) -> Quiet:
        return self

    def __exit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> bool:
        return True

def first(xs: list[int]) -> int:
    with Quiet():
        return xs[0]

print(first([]))
```

```error
main.tpn:10:5: error TP0208: not all paths in 'first' return a value
  help: add a 'return' at the end, or an 'else' branch that returns
```

If `xs` is empty, `Quiet` suppresses the `IndexError` and the function reaches its end without a value. Add a `return` after the `with` for that case. A manager declared `-> None` can't suppress, so a function whose `with` block returns is known to return:

```taipan
from types import TracebackType

class Logged:
    def __enter__(self) -> Logged:
        print("enter")
        return self

    def __exit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> None:
        print("exit")

def first(xs: list[int]) -> int:
    with Logged():
        return xs[0]

print(first([7]))
```

```output
enter
exit
7
```

## `with` in generators and async functions

A `yield` may appear inside a `with` (see [Iterators and generators](../generators/)), and so may an `await`. `async with` uses a manager whose setup and cleanup are themselves async; see [async and await](../async/#async-with).
