---
title: "Errors: try, raise and assert"
description: "Exceptions: raise, try with except, else and finally, chaining with raise from, assert, your own exception classes, and tracebacks."
sidebar:
  order: 13
---

An *exception* is an object that describes an error. Raising one stops the code that's running, and Taipan then looks for a handler: first in the current function, then in the function that called it, and so on outward. A `try` statement is a handler. If nothing handles the exception, the program prints a *traceback*, the list of calls that led to the error, and exits with code 101.

Every error Taipan detects while a program runs, such as a missing dict key or a division by zero, raises an exception, so a `try` can catch it. The built-in exception classes, and which errors raise which, are listed in [Exceptions](../../library/exceptions/).

This page follows [§3.10](../../reference/specification/#310-exceptions-m16), [§3.19](../../reference/specification/#319-exceptions-ii-m41), [§5.6](../../reference/specification/#56-assert-m44) and [§7](../../reference/specification/#7-runtime-errors-panics).

## `raise`

`raise e` raises the exception `e`. `e` must be an instance of a class deriving from `BaseException`, which every exception class does. Its message is the constructor's argument:

```taipan
def check_port(port: int):
    if port < 0:
        raise ValueError(f"bad port {port}")
    print("ok", port)

check_port(80)
check_port(-1)
```

```output
ok 80
```

```error
Traceback (most recent call last):
  File "main.tpn", line 7, in <module>
    check_port(-1)
  File "main.tpn", line 3, in check_port
    raise ValueError(f"bad port {port}")
ValueError: bad port -1
```

A `raise` ends the code it's in, as a `return` does, so a function doesn't need a `return` after it.

The class alone isn't an exception; it has to be called to make one:

```taipan
raise ValueError
```

```error
main.tpn:1:7: error TP0200: cannot raise ValueError
  help: write 'ValueError(…)'
```

Anything other than an exception is `TP0200` too:

```taipan
raise "bad port"
```

```error
main.tpn:1:7: error TP0200: cannot raise str
```

## `try` and `except`

```taipan
let ports = {"api": 8080}
for name in ["api", "db"]:
    try:
        print(name, ports[name])
    except KeyError:
        print(name, "is missing")
```

```output
api 8080
db is missing
```

A `try` statement is the word `try` and a block, then these clauses, in this order:

| Clause | How many | Runs |
|---|---|---|
| `except C:`, `except C as e:`, `except (A, B):` | any number | when the `try` block raises an exception of class `C` (or `A` or `B`), or a class deriving from it. The first clause that matches handles it; the rest are skipped. |
| `except:` | at most one, last of the `except` clauses | when the `try` block raises any `Exception` |
| `else:` | at most one, only after an `except` | when the `try` block finished without raising, and without leaving through `return`, `break` or `continue` |
| `finally:` | at most one, last | always, on every way out of the `try` |

A `try` needs at least one `except` or a `finally`. A `try` with neither, an `else` without an `except`, or a bare `except:` that isn't the last `except` clause is a syntax error (`TP0100`):

```taipan
try:
    print(1)
else:
    print(2)
```

```error
main.tpn:3:1: error TP0100: expected 'except' or 'finally', found 'else'
```

When no clause matches, the exception carries on outward, after the `finally` block runs. Each block is its own scope, so a name declared with `let` or `var` inside the `try` block can't be used in the other clauses or after the `try`; declare it before the `try`, as `port` is below. An exception raised inside an `except` or `else` block isn't caught by the clauses of the same `try`.

```taipan
def port_of(name: str) -> int:
    let ports = {"api": 8080}
    var port = 0
    try:
        port = ports[name]
    except KeyError:
        print("no port for", name)
        return 0
    else:
        print("found", name)
        return port
    finally:
        print("done with", name)

print(port_of("api"))
print(port_of("db"))
```

```output
found api
done with api
8080
no port for db
done with db
0
```

### What `except` can name

- **A class**: `except KeyError:` catches a `KeyError` or anything deriving from it. `except LookupError:` catches both `KeyError` and `IndexError`, since both derive from `LookupError`. The class must be `BaseException` or derive from it (`TP0200` otherwise), and can come from a module: `except zoo.NotFound:`.
- **Several classes**, in parentheses: `except (KeyError, IndexError):` catches either.
- **Nothing**: a bare `except:` catches every `Exception`. It doesn't catch the few classes that derive from `BaseException` directly, such as `SystemExit`, which `sys.exit` raises.
- **A Python exception class**, through its module: `except json.JSONDecodeError:` (see [Python](../../guides/python/)).

```taipan
let xs = [1, 2]
for i in [0, 5]:
    try:
        print(xs[i] // (1 - i))
    except (ZeroDivisionError, IndexError) as e:
        print("caught", e)
```

```output
1
caught list index out of range
```

### `as`

`except C as e:` gives the exception the name `e` inside that clause's block. `e` is a `let`, and doesn't exist after the block. Its type is the clause's class; with several classes, it's the nearest class they all derive from, so `except (KeyError, IndexError) as e` gives `e` the type `LookupError`.

```taipan
try:
    print({"a": 1}["b"])
except KeyError as e:
    print("missing key:", e)
print(e)
```

```error
main.tpn:5:7: error TP0201: undefined name 'e'
```

### Checks on clauses

A clause that can never run, because an earlier clause already catches everything it names, is warning `TP0300`:

```taipan
try:
    print(1 // 0)
except ArithmeticError:
    print("arithmetic")
except ZeroDivisionError:
    print("zero")
```

```output
arithmetic
```

```error
main.tpn:5:8: warning TP0300: unreachable except clause
  help: 'ArithmeticError' is already caught above
```

A clause that catches everything (a bare `except:`, or one naming `Exception` or `BaseException`) also catches mistakes such as an `IndexError` from a wrong index. Unless its block raises again, or binds the exception with `as e` and uses `e`, it silently throws the error away, so it's warning `TP0301`:

```taipan
try:
    print([1, 2][5])
except:
    pass
```

```error
main.tpn:3:1: warning TP0301: this clause catches every exception and discards it
  help: catch a specific class, re-raise with 'raise', or bind it with 'as e' and use it
```

Using the exception, or raising again, shows the catch is deliberate, and the warning goes away:

```taipan
try:
    print([1, 2][5])
except Exception as e:
    print("ignored:", e)
```

```output
ignored: list index out of range
```

## `finally`

The `finally` block runs on every way out of the `try`: when it finishes normally, when it leaves through `return`, `break` or `continue`, and while an exception carries on outward. It's where cleanup goes.

```taipan
def risky(n: int) -> int:
    try:
        if n == 0:
            return 0
        return 10 // (n - 1)
    finally:
        print("cleanup for", n)

print(risky(0))
print(risky(3))
print(risky(1))
```

```output
cleanup for 0
0
cleanup for 3
5
cleanup for 1
```

```error
Traceback (most recent call last):
  File "main.tpn", line 11, in <module>
    print(risky(1))
  File "main.tpn", line 5, in risky
    return 10 // (n - 1)
ZeroDivisionError: integer division or modulo by zero
```

If the `finally` block itself raises, its exception replaces the one in flight. `return`, `break` and `continue` aren't allowed in a `finally` block (`TP0215`), since they would silently discard an exception in flight:

```taipan
def f() -> int:
    try:
        return 1
    finally:
        return 2

print(f())
```

```error
main.tpn:5:9: error TP0215: 'return' cannot be used in a finally block
```

## Re-raising

A bare `raise`, inside an `except` block, raises the exception being handled again, unchanged, so the code can react to an error and still let it carry on:

```taipan
def load(name: str) -> int:
    try:
        return {"a": 1}[name]
    except KeyError:
        print("could not load", name)
        raise

try:
    load("b")
except KeyError as e:
    print("caller saw", e)
```

```output
could not load b
caller saw "b"
```

A bare `raise` anywhere else, including in a function defined inside an `except` block, is `TP0215`:

```taipan
def fail():
    raise

fail()
```

```error
main.tpn:2:5: error TP0215: 'raise' without an exception outside an except block
```

## Chaining: `raise … from …`

Raising an exception while handling another links the two. The new exception's `__context__` is the one being handled, and the traceback shows both:

```taipan
def port_of(name: str) -> int:
    let ports = {"api": 8080}
    try:
        return ports[name]
    except KeyError:
        raise ValueError(f"unknown service {name}")

print(port_of("db"))
```

```error
Traceback (most recent call last):
  File "main.tpn", line 4, in port_of
    return ports[name]
KeyError: "db"

During handling of the above exception, another exception occurred:

Traceback (most recent call last):
  File "main.tpn", line 8, in <module>
    print(port_of("db"))
  File "main.tpn", line 6, in port_of
    raise ValueError(f"unknown service {name}")
ValueError: unknown service db
```

`raise e from c` says the link is deliberate: `c` becomes `e`'s `__cause__`, and the traceback says "direct cause" instead. `c` must be an exception, an optional exception, or `None`. `raise e from None` hides the exception being handled:

```taipan
def port_of(name: str) -> int:
    let ports = {"api": 8080}
    try:
        return ports[name]
    except KeyError as e:
        raise ValueError(f"unknown service {name}") from e

print(port_of("db"))
```

```error
Traceback (most recent call last):
  File "main.tpn", line 4, in port_of
    return ports[name]
KeyError: "db"

The above exception was the direct cause of the following exception:

Traceback (most recent call last):
  File "main.tpn", line 8, in <module>
    print(port_of("db"))
  File "main.tpn", line 6, in port_of
    raise ValueError(f"unknown service {name}") from e
ValueError: unknown service db
```

```taipan
def port_of(name: str) -> int:
    let ports = {"api": 8080}
    try:
        return ports[name]
    except KeyError:
        raise ValueError(f"unknown service {name}") from None

print(port_of("db"))
```

```error
Traceback (most recent call last):
  File "main.tpn", line 8, in <module>
    print(port_of("db"))
  File "main.tpn", line 6, in port_of
    raise ValueError(f"unknown service {name}") from None
ValueError: unknown service db
```

The rules:

- `raise e from c` evaluates `e`, then `c`, sets `e.__cause__` to `c` and `e.__suppress_context__` to `True`, and raises `e`.
- An exception is "being handled" during its `except` block, during a `finally` block that runs because it's carrying on outward, and during an `__exit__` called with it (see [with](../with/)), including in any function called from those. With several, it's the innermost.
- `__context__` is set when the exception is raised. A bare `raise` doesn't change it.
- A `from` value that isn't an exception or `None` is `TP0200`.

```taipan
raise ValueError("bad") from "reason"
```

```error
main.tpn:1:30: error TP0200: 'from' needs an exception or None, found str
```

## What an exception holds

Every exception has these members:

| Member | Type | What it is |
|---|---|---|
| `message` | `str` | the message given to the constructor, `""` when none was |
| `args` | `tuple[str, ...]` | `(message,)`, or `()` when the message is empty; read-only |
| `__cause__` | `BaseException?` | the exception named by `raise … from …`; assigning it also sets `__suppress_context__` |
| `__context__` | `BaseException?` | the exception being handled when this one was raised |
| `__suppress_context__` | `bool` | whether the traceback leaves `__context__` out |
| `__traceback__` | `TracebackType?` | where it was raised and the calls it passed through; `None` until it's raised |

`str(e)` and `print(e)` give the message. A `TracebackType` is one traceback entry, with `tb_lineno: int`, its line, and `tb_next: TracebackType?`, the next entry inward.

```taipan
try:
    try:
        print(1 // 0)
    except ZeroDivisionError as inner:
        raise RuntimeError("wrapped")
except RuntimeError as e:
    print(e.message, e.args)
    print(e.__context__)
    print(e.__cause__)
    if e.__traceback__ is some tb:
        print("line", tb.tb_lineno)
```

```output
wrapped ("wrapped",)
integer division or modulo by zero
None
line 5
```

Exceptions compare by identity, and can be stored, passed and returned like any other value.

## Your own exception classes

An exception class is a class deriving, directly or not, from `Exception`. It follows the usual rules for classes: one with fields of its own sets them in `__init__` and then calls `super().__init__(message)`, and one without an `__init__` takes `Exception`'s message argument.

```taipan
class ConfigError(Exception):
    pass

class NotFound(ConfigError):
    key: str

    def __init__(self, key: str):
        self.key = key
        super().__init__(f"no setting named {key}")

def setting(name: str) -> str:
    raise NotFound(name)

try:
    setting("db.port")
except ConfigError as e:
    print("config error:", e)
    if isinstance(e, NotFound):
        print("key:", e.key)
```

```output
config error: no setting named db.port
key: db.port
```

An exception can also be a dataclass. Its constructor takes its fields, and its message comes from them: empty with no fields, `str()` of the value with one, and the values as a tuple with several.

```taipan
@dataclass
class Missing(Exception):
    key: str

@dataclass
class OutOfRange(Exception):
    value: int
    limit: int

try:
    raise Missing("db")
except Missing as e:
    print(e, "|", e.key)
try:
    raise OutOfRange(12, 10)
except OutOfRange as e:
    print(e, "|", e.value)
```

```output
db | db
(12, 10) | 12
```

A class can't be named after a built-in exception class (`TP0214`), and can't declare a member named after one of the members above.

## `assert`

`assert c` raises `AssertionError` when the condition `c` is `False`, and does nothing otherwise. `assert c, m` raises `AssertionError(str(m))`, and evaluates `m` only when `c` is `False`. An `assert` is always checked; nothing turns it off.

`c` must be a `bool` (`TP0205`), and narrows the code after it as an `if c:` would (see [Optionals and narrowing](../optionals/#where-narrowing-applies)), so `assert x is not None` makes `x` usable as a plain value.

```taipan
def mean(xs: list[float]) -> float:
    assert len(xs) > 0, "mean of no values"
    return sum(xs) / len(xs)

print(mean([1.0, 2.0]))
print(mean([]))
```

```output
1.5
```

```error
Traceback (most recent call last):
  File "main.tpn", line 6, in <module>
    print(mean([]))
  File "main.tpn", line 2, in mean
    assert len(xs) > 0, "mean of no values"
AssertionError: mean of no values
```

Without a message, the traceback ends with `AssertionError` alone. Parentheses around both parts make a tuple, not a condition and a message, so they're an error:

```taipan
let n = 3
assert(n > 5, "too small")
```

```error
main.tpn:2:7: error TP0205: condition must be bool, found tuple[bool, str]
```

## Uncaught exceptions and tracebacks

An exception that nothing catches ends the program. The output printed so far is flushed, then a traceback goes to stderr, and the exit code is **101**. The traceback lists the calls from the outermost to the innermost, each with its file, line, function name and the text of the line. The top level of a file is `<module>`, and a lambda is `<lambda>`. The last line is the class and the message.

```taipan
def average(xs: list[int]) -> float:
    return sum(xs) / len(xs)

def report(xs: list[int]):
    print("average:", average(xs))

report([1, 2, 3])
report([])
```

```output
average: 2.0
```

```error
Traceback (most recent call last):
  File "main.tpn", line 8, in <module>
    report([])
  File "main.tpn", line 5, in report
    print("average:", average(xs))
  File "main.tpn", line 2, in average
    return sum(xs) / len(xs)
ZeroDivisionError: division by zero
```

When the exception has a cause or context, that one is printed first, as in the chaining examples above. The same entry repeated more than three times in a row, as in runaway recursion, is printed three times and then counted:

```taipan
def countdown(n: int) -> int:
    if n == 0:
        raise RuntimeError("liftoff")
    return countdown(n - 1)

countdown(10)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 6, in <module>
    countdown(10)
  File "main.tpn", line 4, in countdown
    return countdown(n - 1)
  File "main.tpn", line 4, in countdown
    return countdown(n - 1)
  File "main.tpn", line 4, in countdown
    return countdown(n - 1)
  [Previous line repeated 7 more times]
  File "main.tpn", line 3, in countdown
    raise RuntimeError("liftoff")
RuntimeError: liftoff
```

`sys.exit(code)` raises `SystemExit`, which ends the program with that exit code (here 0) and no traceback. Since `SystemExit` derives from `BaseException` rather than `Exception`, `except Exception` and a bare `except:` let it through:

```taipan
import sys

try:
    print("exiting")
    sys.exit(0)
except Exception as e:
    print("not reached", e)
finally:
    print("finally still runs")
```

```output
exiting
finally still runs
```

A few failures can't be caught. Importing Python modules happens when the program starts, before any `try` runs, so a failure to load Python or to import a module always ends the program with a `panic:` message and exit code 101. Running out of stack ends the process at once, as it does in .NET; it isn't an exception.
