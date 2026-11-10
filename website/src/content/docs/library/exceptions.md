---
title: Exceptions
description: "The built-in exception classes, what raises each, their fields, defining your own, and the exit code of an uncaught one."
sidebar:
  order: 12
---

An *exception* is an object that describes an error. Raising one, with `raise` or by an operation that fails, stops what the program was doing, and `try` and `except` catch it (see [Errors](../../tour/errors/) in the tour). Every error at run time raises one, so every error can be caught.

Each exception is an instance of a class. The classes form a tree, and `except C` catches an exception of class `C` or of any class below it.

## The classes

```
BaseException
├── SystemExit
├── GeneratorExit
├── asyncio.CancelledError
└── Exception
    ├── ArithmeticError
    │   ├── ZeroDivisionError
    │   └── OverflowError
    ├── LookupError
    │   ├── IndexError
    │   └── KeyError
    ├── ValueError
    │   └── UnicodeError
    │       ├── UnicodeDecodeError
    │       └── UnicodeEncodeError
    ├── TypeError
    ├── NameError
    ├── RuntimeError
    ├── AssertionError
    ├── StopIteration
    ├── StopAsyncIteration
    ├── EOFError
    ├── OSError
    │   ├── FileNotFoundError
    │   ├── FileExistsError
    │   ├── PermissionError
    │   ├── IsADirectoryError
    │   └── TimeoutError
    ├── PythonError
    └── DotNetError
```

Every class's name can be used anywhere without an import, except `asyncio.CancelledError`, which needs `import asyncio`. A class may not be given one of these names.

`BaseException` is the root. `SystemExit`, `GeneratorExit` and `CancelledError` derive from it directly rather than from `Exception`, since they aren't errors: they end the program, a generator or an async function on purpose. So `except Exception` catches every error, but lets those three through.

## What raises each

| Class | Raised by |
|---|---|
| `SystemExit` | `sys.exit` (see [sys and os](../sys-and-os/)) |
| `GeneratorExit` | a generator's `close()`, inside the generator |
| `asyncio.CancelledError` | cancelling an async function (see [asyncio](../asyncio/)) |
| `ZeroDivisionError` | dividing by zero, with `/`, `//` or `%` |
| `OverflowError` | an `int` result too large for 64 bits, and `int(x)` of an infinite `float` |
| `IndexError` | an index past the end of a list, string, tuple or range, and `pop()` on an empty list |
| `KeyError` | a key missing from a dict, a value missing in a set's `remove`, and `pop()` on an empty set |
| `ValueError` | a value of the right type that's wrong: `int(x)` of a NaN, `min` or `max` of nothing, `xs.remove(x)` or `xs.index(x)` with no `x` in `xs`, `s.split("")`, a slice or range step of 0 |
| `UnicodeDecodeError` | reading bytes that aren't valid in a file's encoding (see [Files](../files/)) |
| `UnicodeEncodeError` | writing a character a file's encoding can't hold |
| `TypeError` | an `as` conversion that fails, and `ord(s)` of a string that isn't one character |
| `NameError` | a function reading a top-level variable before it's set, where the compiler can't tell |
| `RuntimeError` | a list, dict or set that gains or loses elements while a loop goes through it |
| `AssertionError` | an `assert` whose condition is `False` |
| `StopIteration` | `next(it)` when the iterator `it` has nothing left |
| `StopAsyncIteration` | the same, for an async iterator |
| `EOFError` | `input()` when the input has ended |
| `OSError` and the classes below it | opening a file the system refuses to open, reading a file opened for writing, and writing one opened for reading (see [Files](../files/)) |
| `TimeoutError` | `asyncio.wait_for` and `asyncio.timeout` running out of time |
| `PythonError` | an exception in Python code the program calls (below) |
| `DotNetError` | an exception in .NET code the program calls, when it's caught as a Taipan class (below) |
| `BaseException` | nothing directly: the base of every exception class, for catching all of them |
| `Exception` | nothing directly: the base of every class but `SystemExit`, `GeneratorExit` and `asyncio.CancelledError`, for catching any error |
| `ArithmeticError` | nothing directly: the base of `ZeroDivisionError` and `OverflowError`, for catching both |
| `LookupError` | nothing directly: the base of `IndexError` and `KeyError`, for catching both |
| `UnicodeError` | nothing directly: the base of `UnicodeDecodeError` and `UnicodeEncodeError`, for catching both |
| `OSError` | opening a file that can't be opened, when no class below says why more exactly; the base of the four below |
| `FileNotFoundError` | opening a file that doesn't exist, for reading |
| `FileExistsError` | opening a file that already exists, with mode `"x"` |
| `PermissionError` | opening a file the program may not read or write |
| `IsADirectoryError` | opening a directory as a file |

`ArithmeticError`, `LookupError`, `UnicodeError`, `Exception` and `BaseException` aren't raised themselves, only the classes below them: they're for catching a group. A program may raise any class, as in `raise ValueError("bad port")`.

## Catching a group

Since `except` catches the classes below the one it names, `except LookupError` catches both a missing key and an index out of range:

```taipan
def risky(step: int) -> int:
    let ports = {"api": 8080}
    let names = ["api"]
    if step == 0:
        return ports["db"]
    elif step == 1:
        return len(names[5])
    return 10 // 0

for step in range(3):
    try:
        print(risky(step))
    except LookupError as e:
        print("lookup failed:", e)
    except ArithmeticError as e:
        print("arithmetic failed:", e)
```

```output
lookup failed: "db"
lookup failed: list index out of range
arithmetic failed: integer division or modulo by zero
```

The first clause that matches is the one that runs, so a clause for a class below one already caught can never run, and the compiler warns about it. `except (KeyError, IndexError) as e` catches either of two classes, and a bare `except:` is the same as `except Exception:`.

## Fields

| Field | Gives |
|---|---|
| `e.message` | the message, a `str`; `""` if there's none |
| `e.args` | a `tuple[str, ...]`: `(message,)`, or `()` when the message is empty |
| `e.__cause__` | the exception given with `raise … from …`, or `None` |
| `e.__context__` | the exception that was being handled when this one was raised, or `None` |
| `e.__traceback__` | where it was raised, and the calls it passed through, or `None` before it's raised |

`str(e)` and `print(e)` give the message. Inside a list or other container, an exception displays with its class, as `ValueError("bad port")`. Every built-in class except `SystemExit` is made from a message, which may be left out: `ValueError("bad port")`, `KeyError()`.

```taipan
let err = ValueError("bad port")
print(err, "|", err.message, "|", err.args)
print([err])
print(KeyError().args)
```

```output
bad port | bad port | ("bad port",)
[ValueError("bad port")]
()
```

Some classes have more fields:

| Class | Fields |
|---|---|
| `SystemExit` | `code`, the exit code, an `int` |
| `OSError` and the classes below it | `errno`, the error's number, an `int?`; `strerror`, its description, a `str?`; `filename`, the path, a `str?` (see [Files](../files/)) |
| `StopIteration` | `value`, what the generator that ended returned, an `object`, or `None` |
| `PythonError` | `type`, the Python exception's class name, a `str`; `value`, the Python exception itself, a `py` |
| `DotNetError` | `type`, the .NET exception's full class name, such as `System.FormatException`, a `str` |

## Python and .NET exceptions

A program that calls Python code (see [Python](../../guides/python/)) gets a `PythonError` when that code raises an exception. Its `type` is the Python class's name, and its `message` is Python's message:

```taipan
from python import json

try:
    json.loads("{not json")
except PythonError as e:
    print(e.type)
    print(e.message)
```

```output
JSONDecodeError
Expecting property name enclosed in double quotes: line 1 column 2 (char 1)
```

Taipan's classes are its own, so `except KeyError` doesn't catch a `KeyError` from Python code, and the compiler warns when a `try` looks like it expects it to. To catch one Python class, name it through its module, as `except json.JSONDecodeError`, or for Python's built-in classes `except builtins.KeyError` after `from python import builtins`. Nothing can construct a `PythonError` or derive a class from it but the runtime.

An exception from .NET code (see [.NET](../../guides/dotnet/)) can be caught by its .NET class. Caught as `Exception`, `DotNetError` or any other Taipan class, it's a `DotNetError`:

```taipan
from dotnet.System import Int32

try:
    print(Int32.Parse("forty"))
except DotNetError as e:
    print(e.type)
    print(e.message)
```

```output
System.FormatException
The input string 'forty' was not in a correct format.
```

Like `PythonError`, a `DotNetError` is made only by the runtime.

## Defining your own

An exception class is a class that derives from `Exception`, or from one of the classes below it. One without an `__init__` takes a message, as the built-in classes do. One with fields of its own sets them in its `__init__` and then calls `super().__init__(message)`:

```taipan
class ConfigError(Exception):
    pass

class MissingSetting(ConfigError):
    name: str

    def __init__(self, name: str):
        self.name = name
        super().__init__(f"setting {name!r} is missing")

def check(settings: dict[str, str]):
    if "port" not in settings:
        raise MissingSetting("port")
    if int(settings["port"]) is None:
        raise ConfigError("port must be a number")

for settings in [{"host": "x"}, {"port": "eighty"}, {"port": "80"}]:
    try:
        check(settings)
        print("ok")
    except MissingSetting as e:
        print("missing:", e, "|", e.name)
    except ConfigError as e:
        print("config error:", e)
```

```output
missing: setting "port" is missing | port
config error: port must be a number
ok
```

An exception class may also be a [dataclass](../../tour/classes/), deriving from a built-in class or from another dataclass exception, but not from a plain class like `ConfigError` above. Its message comes from its fields: the field's value with one field, and all of them, as a tuple, with several:

```taipan
@dataclass
class BadValue(ValueError):
    name: str
    value: str

try:
    raise BadValue("port", "eighty")
except ValueError as e:
    print(e)
    if isinstance(e, BadValue):
        print(e.name, e.value)
```

```output
("port", "eighty")
port eighty
```

No class may derive from `PythonError` or `DotNetError`, and a dataclass may not derive from `SystemExit`. A class may derive from `BaseException` directly, for something that `except Exception` shouldn't catch, though that's rarely what's wanted.

## Uncaught exceptions

An exception that nothing catches ends the program. It prints a *traceback* to the error output: each call the exception passed through, from the outermost to where it was raised, then its class and message. The program then exits with code **101**, whatever the exception. (Python exits with 1.) What the program printed before still appears.

With `raise … from e`, the traceback shows the first exception, then the one that replaced it:

```taipan
class NotFound(Exception):
    pass

def lookup(ports: dict[str, int], name: str) -> int:
    try:
        return ports[name]
    except KeyError as e:
        raise NotFound(f"no service named {name!r}") from e

print("looking")
print(lookup({}, "db"))
```

```output
looking
```

```error
Traceback (most recent call last):
  File "main.tpn", line 6, in lookup
    return ports[name]
KeyError: "db"

The above exception was the direct cause of the following exception:

Traceback (most recent call last):
  File "main.tpn", line 11, in <module>
    print(lookup({}, "db"))
  File "main.tpn", line 8, in lookup
    raise NotFound(f"no service named {name!r}") from e
NotFound: no service named "db"
```

`SystemExit` is the one exception that ends the program without a traceback, with its own exit code (see [sys and os](../sys-and-os/)).

Recursion that never ends isn't an exception: it ends the program at once, and nothing can catch it. There's no `RecursionError`.
