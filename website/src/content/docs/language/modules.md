---
title: Modules and imports
description: "Splitting a program into files: modules, import and from … import, what a module exports, built-in modules, and importing Python and .NET."
sidebar:
  order: 12
---

A program can be split into several files. The file you run is the main file; each other file is a module, which the main file or another module imports to use what it defines. The rules are defined in [§4.7](../../reference/specification/#47-modules-m9) of the specification.

## Modules are files

A module is a `.tpn` file in the same directory as the main file. Its name is the file's name without `.tpn`, so `shapes.tpn` is the module `shapes`. There are no subdirectories of modules and no dotted module names.

A module holds only declarations: `import`s, functions, classes, enums, constants, protocols and type aliases. It has no code of its own that runs; only the main file's top-level statements run.

The main file can't be imported, by a module or by itself (`TP0222`): it runs once, as the program, so it has nothing to export. Put what the main file and a module both need into a module that each imports.

The examples on this page use this module, `shapes.tpn`:

```taipan title="shapes.tpn"
const UNIT = "cm"

type Size = tuple[float, float]

enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)

class Canvas:
    items: list[Shape] = []

    def add(self, s: Shape):
        self.items.append(s)

def area(s: Shape) -> float:
    match s:
        Circle(r): return 3.14159 * r * r
        Rect(w, h): return w * h

def size_of(s: Shape) -> Size:
    match s:
        Circle(r): return 2 * r, 2 * r
        Rect(w, h): return w, h
```

## `import`

`import shapes` makes the module's names available as `shapes.` followed by the name:

```taipan
import shapes

let c = shapes.Canvas()
c.add(shapes.Circle(1))
c.add(shapes.Shape.Rect(2, 3))
for s in c.items:
    let size: shapes.Size = shapes.size_of(s)
    print(f"{shapes.area(s):.2f} {shapes.UNIT}², {size}")
```

```output
3.14 cm², (2.0, 2.0)
6.00 cm², (2.0, 3.0)
```

The qualified names work everywhere a name does:

| Where | Form |
|---|---|
| expressions | `shapes.area(s)`, `shapes.UNIT`, `shapes.Canvas()`, `shapes.Circle(1)`, `shapes.Shape.Circle(1)` |
| types | `shapes.Shape`, `shapes.Size`, and for generic types `shapes.Box[int]` |
| base classes | `class Board(shapes.Canvas):` |
| `match` patterns | `shapes.Shape.Circle(r)` |
| `except` clauses | `except shapes.NotFound:` |

`import shapes as s` binds the module under another name, `s`, instead.

## `from … import`

`from shapes import area, Circle` binds the listed names directly, without the module's name in front. Importing an enum also makes its variants usable alone. `as` renames a name as it's imported:

```taipan
from shapes import Shape, Canvas, area as area_of, UNIT

let shapes_list: list[Shape] = [Circle(1), Rect(2, 3)]
for s in shapes_list:
    print(f"{area_of(s):.2f} {UNIT}²")
```

```output
3.14 cm²
6.00 cm²
```

There's no `from shapes import *`: each name is listed, so a reader can see where every name comes from.

```taipan
from shapes import *
```

```error
main.tpn:1:20: error TP0100: expected name, found '*'
```

## What a module exports

A module exports every name it defines at its top level: functions, classes, enums and their variants, constants, protocols and type aliases. There's no way to make a name private to its module.

A name that a module imported isn't exported by it, so it can't be imported from that module in turn. Import it from the module that defines it:

```taipan title="report.tpn"
from shapes import area

def summary(total: float) -> str:
    return f"total {total:.1f}"
```

```taipan
from report import summary, area
```

```error
main.tpn:1:29: error TP0201: module 'report' has no definition 'area'
```

A name the module doesn't define, and a module file that doesn't exist, are both errors at the import:

```taipan
from shapes import volume
```

```error
main.tpn:1:20: error TP0201: module 'shapes' has no definition 'volume'
```

```taipan
import shapez
```

```error
main.tpn:1:8: error TP0221: module 'shapez' not found
  help: looked for shapez.tpn; a Python module is imported with 'from python import shapez'
```

## Modules have no top-level statements

A `let`, `var`, `print(…)` or any other statement at a module's top level is an error, since only the main file's top level runs. Put module-wide values in a `const`, and code in functions:

```taipan title="settings.tpn"
let debug = True

def level() -> int:
    return 1
```

```taipan
import settings

print(settings.level())
```

```error
settings.tpn:1:1: error TP0222: only the main file can have top-level statements
```

For the same reason, a top-level `let` or `var` of the main file is visible to the main file's functions, but not to any module.

There's no `if __name__ == "__main__":` either. The file given to `taipan run` is the program's starting point; code to share belongs in a module.

```taipan
if __name__ == "__main__":
    print("run directly")
```

```error
main.tpn:1:4: error TP0201: undefined name '__name__'
  help: Taipan has no '__name__': a main file can't be imported, so put code to share in a module
```

## Rules for imports

- An import may appear anywhere at a file's top level, not only at the start, and takes effect throughout the file. An import inside a function is an error.
- Imported names share the file's top-level names: an import can't have the name of a function, class or constant the file defines, or of another import.
- A module's name is only for reaching its names: `print(shapes)` is an error.
- Modules may import each other in a cycle: `a.tpn` may import `b.tpn` while `b.tpn` imports `a.tpn`, and their functions may call each other.
- Constants are set module by module before the main file's top level runs, each module after the ones it imports.
- An error in a module is reported with that module's file name.

```taipan
from shapes import area

def area(width: float, height: float) -> float:
    return width * height
```

```error
main.tpn:1:20: error TP0214: duplicate definition of 'area'
```

```taipan
def main():
    import shapes
```

```error
main.tpn:2:5: error TP0100: expected statement, found 'import'
```

```taipan
import shapes

print(shapes)
```

```error
main.tpn:3:7: error TP0200: 'shapes' is a module, not a value
```

Two modules that import each other:

```taipan title="ping.tpn"
import pong

def ping(n: int) -> str:
    if n == 0:
        return "done"
    return "ping " + pong.pong(n - 1)
```

```taipan title="pong.tpn"
import ping

def pong(n: int) -> str:
    if n == 0:
        return "done"
    return "pong " + ping.ping(n - 1)
```

```taipan
from ping import ping

print(ping(4))
```

```output
ping pong ping pong done
```

## Built-in modules

Four modules are part of Taipan and need no file: `sys`, `os`, `types` and `asyncio`. They're imported like any other module, and a file named `sys.tpn` beside the main file doesn't replace them.

| Module | What it has | Page |
|---|---|---|
| `sys` | `argv`, `exit`, `stdin`, `stdout`, `stderr` | [sys and os](../../library/sys-and-os/) |
| `os` | `environ`, `getenv` | [sys and os](../../library/sys-and-os/) |
| `types` | `TracebackType`, for `__exit__` | [with](../with/) |
| `asyncio` | `run`, `gather`, `sleep`, `wait_for`, `timeout`, `CancelledError`, `TimeoutError` | [asyncio](../../library/asyncio/) |

```taipan
import sys
from os import getenv as env

print(len(sys.argv) >= 1, env("TAIPAN_DOCS_UNSET") ?? "unset")
```

```output
True unset
```

## Importing Python and .NET

Two module names are reserved for code that isn't written in Taipan:

- `from python import json` imports a Python module, whose values have the type `py`. See [Calling Python](../../guides/python/). A plain `import json` looks for a Taipan module `json.tpn` and fails (`TP0221`), and its help says to use this form. Python's `typing`, `typing_extensions`, `dataclasses`, `abc` and `__future__` aren't imported at all: `Callable`, `@dataclass`, `ABC` and `Protocol` are built in, and `from enum import Enum` is replaced by `enum Name:`.
- `from dotnet.System.Text import StringBuilder` imports types from a .NET namespace, and `import dotnet.System.IO as io` binds a whole namespace (the `as` is required). See [Using .NET](../../guides/dotnet/).

```taipan
from python import json

let data = json.loads("[1, 2, 3]")
print(len(data as list[int]))
```

```output
3
```

```taipan
from dotnet.System.Text import StringBuilder

let sb = StringBuilder()
sb.Append("built with .NET")
print(sb.ToString())
```

```output
built with .NET
```

`import python` and `import dotnet` alone are errors whose help shows the form to use.
