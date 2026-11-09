---
title: sys and os
description: "The program's arguments, ending it with an exit code, the standard streams, and environment variables."
sidebar:
  order: 10
---

`sys` and `os` are modules that come with Taipan. `sys` holds what a program gets from whoever ran it, its arguments and its input and output, and ends it with an exit code. `os` reads the environment variables. A program imports them before it uses them:

```taipan
import sys
from os import getenv
```

Each has only the names listed here. Any other, such as `sys.version` or `os.path`, is a compile error.

## sys

| Name | Gives |
|---|---|
| `sys.argv` | a `list[str]`: the program's path, then each of its arguments |
| `sys.exit()` | ends the program with exit code 0 |
| `sys.exit(code)` | ends the program with the exit code `code`, an `int` |
| `sys.exit(message)` | writes `message`, a `str`, to the error output, and ends the program with exit code 1 |
| `sys.stdin` | the program's input, as a file |
| `sys.stdout` | the program's output, as a file: where `print` writes |
| `sys.stderr` | the program's error output, as a file: where error messages go |

### Arguments

`sys.argv[0]` is the program's path, and the arguments come after it, so `sys.argv[1:]` is the arguments alone. `taipan run` passes what follows `--` as the arguments:

```
taipan run copy.tpn -- notes.txt backup.txt
```

There, `sys.argv` is `["copy.tpn", "notes.txt", "backup.txt"]`. For a program built with `taipan build`, `sys.argv[0]` is the built program's path. `sys.argv` is an ordinary list, which the program may change.

### Exit codes

A program's *exit code* is a number it gives back when it ends, which tells whatever ran it, such as a shell script, whether it worked. 0 means it did, and any other number means it didn't. A Taipan program ends with one of these:

| Exit code | When |
|---|---|
| 0 | the program ran to its end, or called `sys.exit()` |
| `code` | the program called `sys.exit(code)` |
| 1 | the program called `sys.exit(message)` |
| 101 | an exception that nothing caught ended it, after printing a traceback (see [Exceptions](../exceptions/)) |

`taipan run` itself ends with 1 when the program doesn't compile, and with 2 when it's used wrongly, as when the file doesn't exist.

On Linux and macOS, only the lowest 8 bits of the code reach the program that ran it, so it's a number from 0 to 255: `sys.exit(300)` gives 44, and `sys.exit(-1)` gives 255.

`sys.exit(message)` is a short way to stop with an explanation:

```taipan
import sys

if len(sys.argv) != 3:
    sys.exit("usage: copy <source> <target>")
```

Run without arguments, it writes `usage: copy <source> <target>` to the error output and ends with exit code 1.

### How exit works

`sys.exit` doesn't stop the program on the spot: it raises a `SystemExit` exception, and the program ends when that reaches the top. So `finally` blocks run on the way, and a `with` block closes its file. `SystemExit` has a field `code`, the exit code, and its message is the code as text, or the `message` given.

`SystemExit` doesn't derive from `Exception` (see [Exceptions](../exceptions/)), so `except Exception` doesn't catch it, and a handler meant for errors doesn't stop the program from ending. `except SystemExit` does catch it:

```taipan
import sys

for code in [0, 3]:
    try:
        sys.exit(code)
    except SystemExit as e:
        print("exit code", e.code)
try:
    sys.exit("usage: copy <source> <target>")
except SystemExit as e:
    print(e.code, e)
try:
    try:
        sys.exit()
    except Exception as e:
        print("not caught here", e)
except SystemExit as e:
    print("caught outside", e.code)
```

```output
exit code 0
exit code 3
1 usage: copy <source> <target>
caught outside 0
```

When a `SystemExit` reaches the top, the program ends without a traceback. Everything it printed is written out first, and its open files are closed.

### Standard streams

`sys.stdin`, `sys.stdout` and `sys.stderr` are files, with the methods of [Files](../files/): `sys.stdout.write(s)`, `sys.stdin.readline()`, `for line in sys.stdin:` and the others. They're all UTF-8.

```taipan
import sys

print("result: 42")
print("warning: no config file", file=sys.stderr)
sys.stdout.write("done\n")
```

```output
result: 42
done
```

```error
warning: no config file
```

## os

| Name | Gives |
|---|---|
| `os.environ` | a `dict[str, str]` of the environment variables' names and values |
| `os.getenv(name)` | the value of the variable `name`, as a `str?`: `None` if it isn't set |
| `os.getenv(name, default)` | the value of `name`, or `default` if it isn't set, as a `str` |

*Environment variables* are names and values that a program gets from whatever ran it, such as `PATH` or `HOME`, often used for settings.

`os.environ` holds them as they were when the program started, in the order the system gives them. It's an ordinary dict (see [dict](../dict/)), and `os.getenv` reads it, so a change to it is seen by later calls to `getenv`. A change isn't seen by .NET code or Python modules the program uses, which read the variables themselves. On Windows the names in `os.environ` are in capitals, and looking one up ignores case, so `"PATH" in os.environ` is `True` there too, though Windows spells it `Path`.

`os.getenv(name)` gives an optional, which has to be checked before it's used (see [Optionals](../../tour/optionals/)). Give a default, or use `is some` or `??`:

```taipan
import os

print("PATH" in os.environ)
print(os.getenv("TAIPAN_DOCS_COLOR"))
print(os.getenv("TAIPAN_DOCS_COLOR", "blue"))
os.environ["TAIPAN_DOCS_COLOR"] = "green"
if os.getenv("TAIPAN_DOCS_COLOR") is some color:
    print("color is", color)
let port = int(os.getenv("TAIPAN_DOCS_PORT") ?? "8080") ?? 80
print(port)
```

```output
True
None
blue
color is green
8080
```

The other parts of Python's `os` module, such as `os.path` and working with folders, aren't in Taipan yet.
