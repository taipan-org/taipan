---
title: Files
description: "Text files: open() and its modes, reading and writing, with, the standard streams, and the errors opening raises."
sidebar:
  order: 9
---

`open(path)` opens a text file and gives a `TextIO`, an open file that reads or writes text a line or a piece at a time. Every file is a text file: binary files aren't supported yet.

A file should be closed when the program is done with it, and the simplest way is `with`, which closes it at the end of its block, however the block ends:

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-hello.txt"
with open(path, "w") as f:
    f.write("hello\n")
with open(path) as f:
    print(f.read())
```

```output
hello

```

The examples on this page write their files to the system's temporary folder, so they can run anywhere. A program usually takes its paths from its arguments, as `sys.argv` (see [sys and os](../sys-and-os/)).

## Opening

| Call | Gives |
|---|---|
| `open(path)` | a `TextIO` for reading the file at `path` |
| `open(path, mode)` | a `TextIO` opened as `mode` says, below |
| `open(path, mode, encoding=e)` | the same, reading and writing text in the encoding `e`, `"utf-8"` if it isn't given |
| `open(path, mode, newline="")` | the same, leaving line endings as they are |

| Mode | Means |
|---|---|
| `"r"` | read; the default. The file must exist |
| `"w"` | write, emptying the file first, or creating it if it doesn't exist |
| `"a"` | append: write after what's already there, creating the file if it doesn't exist |
| `"x"` | write a new file; raises `FileExistsError` if it already exists |

A `t` may follow the mode, as in `"rt"`, and changes nothing. The mode must be written in the call, as text in quotes, so the compiler can check it: any other mode is a compile error, and so is a mode held in a variable. A `b` (binary) or `+` (read and write) mode isn't supported yet.

`encoding` is one of `"utf-8"`, `"utf-8-sig"`, `"ascii"` and `"latin-1"`, also written in the call. Other spellings of the same ones, such as `"utf8"`, `"UTF-8"` and `"latin1"`, work too. `"utf-8-sig"` skips the byte order mark some programs put at the start of a file, and writes one first.

`newline` is `None`, the default, or `""`:

- With `None`, reading ends a line at `\n`, `\r\n` or `\r` and gives each line ending as `\n`, so a program sees the same text from a file made on Windows or elsewhere. Writing turns each `\n` into the system's line ending, `\r\n` on Windows and `\n` elsewhere.
- With `""`, reading ends lines at the same three endings but leaves them as they are, and writing writes `\n` unchanged.

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-modes.txt"
with open(path, "w", newline="") as f:
    f.write("first\r\nsecond\n")
with open(path, "a", newline="") as f:
    f.write("third\n")
with open(path) as f:
    print(f.readlines())
with open(path, newline="") as f:
    print(f.readlines())
```

```output
["first\n", "second\n", "third\n"]
["first\r\n", "second\n", "third\n"]
```

## Reading

| Method | Gives |
|---|---|
| `f.read()` | the rest of the file, as one `str`; `""` at the end |
| `f.read(n)` | at most the next `n` characters; `""` at the end |
| `f.readline()` | the next line, with its line ending; `""` at the end |
| `f.readlines()` | the rest of the file as a `list[str]` of lines, each with its line ending |

A file is also an iterator over its lines (see [Iterators and generators](../../tour/iterators-and-generators/)), so `for line in f:` goes through the lines that are left, each with its line ending, and `list(f)` and `next(f)` work. Every way of reading moves on through the file: a line that `readline` gave isn't seen again by a loop.

`line.rstrip("\n")` removes a line's ending, and `line.strip()` removes it along with spaces at either end (see [str](../str/)).

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-read.txt"
with open(path, "w") as f:
    f.write("apple\nbanana\ncherry\n")
with open(path) as f:
    print(repr(f.readline()))
    print(repr(f.read(3)))
    print(f.readlines())
    print(repr(f.read()))
with open(path) as f:
    for line in f:
        print(line.rstrip("\n"))
with open(path) as f:
    print(repr(next(f)), list(f))
```

```output
"apple\n"
"ban"
["ana\n", "cherry\n"]
""
apple
banana
cherry
"apple\n" ["banana\n", "cherry\n"]
```

## Writing

| Method | Gives |
|---|---|
| `f.write(s)` | writes `s`, and gives the number of characters written |
| `f.writelines(lines)` | writes each string in `lines`, a list or any other iterable, adding nothing between them |
| `f.flush()` | writes out anything still held in memory, so other programs can see it |
| `f.close()` | flushes and closes the file; closing a closed file does nothing |

`write` doesn't add a line ending, so each line needs its own `\n`. `print(…, file=f)` writes to a file as `print` writes to the screen, with a line ending.

Writing is held in memory and written out in pieces, which is faster. `flush` and `close` write out what's held, and so does the end of a `with` block. A file left open is flushed and closed when the program ends, even when it ends with an error.

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-write.txt"
with open(path, "w") as f:
    print(f.write("one\n"))
    f.writelines(["two\n", "three\n"])
    print("four", file=f)
with open(path) as f:
    print(f.read(), end="")
```

```output
4
one
two
three
four
```

## Attributes

| Attribute | Gives |
|---|---|
| `f.name` | the path, as it was given to `open` |
| `f.mode` | the mode, as it was given to `open`: `"r"` when none was |
| `f.encoding` | the encoding, as it was given to `open`: `"utf-8"` when none was |
| `f.closed` | whether the file has been closed |

A file displays as `<_io.TextIOWrapper name='notes.txt' mode='r' encoding='utf-8'>`.

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-attributes.txt"
with open(path, "w", encoding="utf8") as f:
    print(f.name == path, f.mode, f.encoding, f.closed)
let f = open(path)
print(f.mode, f.encoding)
f.close()
print(f.closed)
```

```output
True w utf8 False
r utf-8
True
```

## Standard streams

`sys.stdin`, `sys.stdout` and `sys.stderr` are files that are already open: the program's input, its output, and its error output, where error messages go. They're all UTF-8.

- `print` writes to `sys.stdout`, and `print(…, file=sys.stderr)` to `sys.stderr`.
- `input()` reads a line from `sys.stdin`. `for line in sys.stdin:` reads every line of the input, until it ends.
- Their names are `<stdin>`, `<stdout>` and `<stderr>`. `sys.stdin` reads line endings as `newline=None` does, on every system.
- Closing one makes its methods raise `ValueError` from then on, so programs don't close them.

```taipan
import sys

print("to the output")
print("to the error output", file=sys.stderr)
sys.stdout.write("written\n")
print(sys.stdin.name, sys.stdout.name, sys.stderr.name)
```

```output
to the output
written
<stdin> <stdout> <stderr>
```

```error
to the error output
```

This program numbers the lines of its input:

```taipan
import sys

var count = 0
for line in sys.stdin:
    count += 1
    print(count, line.rstrip("\n"))
```

Given the two lines `alpha` and `beta` as input, it prints `1 alpha` and `2 beta`.

## Errors

When the system refuses to open a file, `open` raises an `OSError`, or one of the classes that derive from it, which says why:

| Class | Raised when | `errno` |
|---|---|---|
| `FileNotFoundError` | the file doesn't exist, for mode `"r"` | 2 |
| `PermissionError` | the program isn't allowed to open it | 13 |
| `FileExistsError` | the file already exists, for mode `"x"` | 17 |
| `IsADirectoryError` | the path is a folder, not a file | 21 |
| `OSError` | any other refusal | `None` |

Each has three more fields than other exceptions: `errno`, the error's number (an `int?`), `strerror`, its description (a `str?`), and `filename`, the path (a `str?`). Their message gives all three, as in `[Errno 2] No such file or directory: 'notes.txt'`. The numbers and descriptions are the same on every system. A refusal that isn't one of the four is a plain `OSError`, whose `errno` and `strerror` are `None` and whose message is .NET's.

Catching `OSError` catches every one of them, since `except` catches a class and the classes that derive from it. Catch a single class to handle only that case:

```taipan
import os

let folder = os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp"
try:
    with open("no-such-file.txt") as f:
        print(f.read())
except FileNotFoundError as e:
    print(e)
    print(e.errno, e.strerror, e.filename)

let path = folder + "/taipan-docs-exists.txt"
with open(path, "w") as f:
    f.write("one\n")
try:
    with open(path, "x") as f:
        f.write("two\n")
except FileExistsError as e:
    print("already there:", e.errno)

try:
    let f = open(folder)
    f.close()
except OSError as e:
    print("can't open it:", e.strerror)
```

```output
[Errno 2] No such file or directory: 'no-such-file.txt'
2 No such file or directory no-such-file.txt
already there: 17
can't open it: Is a directory
```

An error nothing catches ends the program with a traceback and exit code 101 (see [Errors](../../tour/errors/)):

```taipan
with open("settings.txt") as f:
    print(f.read())
```

```error
Traceback (most recent call last):
  File "main.tpn", line 1, in <module>
    with open("settings.txt") as f:
FileNotFoundError: [Errno 2] No such file or directory: 'settings.txt'
```

A file raises other errors once it's open:

| Error | Raised when |
|---|---|
| `OSError`, `not readable` | reading a file opened for writing |
| `OSError`, `not writable` | writing a file opened for reading |
| `ValueError`, `I/O operation on closed file.` | using a file after it's closed |
| `UnicodeDecodeError` | reading bytes that aren't valid in the file's encoding |
| `UnicodeEncodeError` | writing a character the file's encoding can't hold, such as `é` in `"ascii"` |

```taipan
import os

let path = (os.getenv("TMPDIR") ?? os.getenv("TEMP") ?? "/tmp") + "/taipan-docs-errors.txt"
with open(path, "w", encoding="latin-1") as f:
    f.write("café\n")
try:
    with open(path) as f:
        f.write("more")
except OSError as e:
    print(e)
try:
    with open(path) as f:
        print(f.read())
except UnicodeDecodeError as e:
    print(e)
with open(path, encoding="latin-1") as f:
    print(f.read(), end="")
    f.close()
    try:
        print(f.read())
    except ValueError as e:
        print(e)
```

```output
not writable
'utf-8' codec can't decode byte 0xe9
café
I/O operation on closed file.
```

Files have no `seek` or `tell`, so a file is read from the start to the end once. To read it again, open it again.
