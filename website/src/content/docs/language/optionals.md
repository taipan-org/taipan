---
title: Optionals and narrowing
description: "T? and None, the tests is None, is not None and is some, the ?? operator, and every rule for narrowing."
sidebar:
  order: 8
---

A value that might be missing has an *optional* type, written `T?`: a `str?` holds either a `str` or `None`. Only an optional can hold `None`. Before an optional can be used as the value inside it, the program has to check that it isn't `None`, and the compiler makes sure it does. After such a check the compiler treats the value as a plain `T`; this is called *narrowing*.

Optionals are defined in [§3.4](../../reference/specification/#34-optionals-and-narrowing), and narrowing in [§3.9](../../reference/specification/#39-narrowing-m15-m27) and [§3.18](../../reference/specification/#318-types-ii-m40).

## `T?` and `None`

`T?` is `T` or `None`. A `T` can be used wherever a `T?` is expected, and so can `None`. The reverse isn't allowed: a `T?` used where a `T` is expected is error `TP0203`, and so is using its fields, methods or indexing, looping over it, or using it with an operator other than `==` and `!=`.

```taipan
def find(names: list[str], prefix: str) -> str?:
    for name in names:
        if name.startswith(prefix):
            return name
    return None

let found = find(["ada", "grace"], "gr")
print(len(found))
```

```error
main.tpn:8:11: error TP0203: str? must be narrowed before use
  help: narrow it with 'is some', or give a default with '??'
```

`None` is spelled with a capital N. `none` is an ordinary name, so writing it by mistake is an undefined name:

```taipan
let port: int? = none
```

```error
main.tpn:1:18: error TP0201: undefined name 'none'
  help: use 'None'
```

`None` on its own doesn't say which optional it is, so a binding that starts as `None` needs a type:

```taipan
let port = None
```

```error
main.tpn:1:12: error TP0206: cannot infer the type of None
  help: add a type annotation
```

```taipan
let port: int? = None
print(port)
```

```output
None
```

`==` and `!=` compare a `T?` with a `T`, another `T?` or `None`. Two optionals are equal when both are `None` or both hold equal values.

```taipan
let a: int? = 3
let b: int? = None
print(a == 3, b == None, a == b, b != 3)
```

```output
True True False True
```

## Optionals don't nest

There is no `T??`: an optional either holds a value or is `None`, and there is no "missing optional" in between. Writing `T??` is error `TP0213`.

```taipan
let x: int?? = None
```

```error
main.tpn:1:8: error TP0213: optional types cannot be nested
  help: write 'T?' once; an optional can't hold another optional
```

So the result type never doubles up. Looking a key up with `get` on a dict whose values are already optional gives that same optional, and a missing key and a stored `None` both give `None`:

```taipan
let limits: dict[str, int?] = {"cpu": 4, "disk": None}
print(limits.get("cpu"), limits.get("disk"), limits.get("gpu"))
```

```output
4 None None
```

In a list or dict literal without a declared type, mixing `T` and `None`, or `T` and `T?`, gives `T?` elements:

```taipan
let xs = [1, None, 3]
var total = 0
for x in xs:
    total += x ?? 0
print(xs, total)
```

```output
[1, None, 3] 4
```

## `is None` and `is not None`

`x is None` is `True` when `x` holds no value, and `x is not None` when it does. Both give a `bool`, so they can be used in any expression, not only in an `if`. The tested value must be optional (`TP0200` otherwise), and it's evaluated once.

```taipan
def missing(x: int?) -> bool:
    return x is None

print(missing(None), missing(4))
```

```output
True False
```

```taipan
let n = 5
if n is None:
    print("none")
```

```error
main.tpn:2:4: error TP0200: 'is None' needs an optional, found int
```

`x is not None` narrows `x` where it's true, and `x is None` narrows `x` where it's false: in the `else` branch, or after an `if` whose branch can't continue (see [where narrowing applies](#where-narrowing-applies)).

```taipan
def describe(port: int?) -> str:
    if port is None:
        return "no port"
    else:
        return f"port {port + 1}"

print(describe(None))
print(describe(8080))
```

```output
no port
port 8081
```

`x == None` and `x != None` mean the same as `x is None` and `x is not None`, and narrow the same way.

## `is some`

`if x is some v:` tests whether `x` holds a value and, in the branch that runs when it does, gives that value the name `v`, of type `T`. It works on anything optional, including a call's result, which `is not None` can't narrow.

```taipan
def find(names: list[str], prefix: str) -> str?:
    for name in names:
        if name.startswith(prefix):
            return name
    return None

let names = ["ada", "grace", "alan"]
if find(names, "gr") is some name:
    print(name.upper())
elif find(names, "a") is some other:
    print("other:", other)
else:
    print("nobody")
```

```output
GRACE
```

The rules:

- `is some` can be the whole condition of an `if`, an `elif` or a `while`, and nothing else. Anywhere else, such as `let found = x is some v`, it's a syntax error (`TP0100`).
- The new name is a `let`, so it can't be assigned, and it can't reuse a name already declared in the same scope (`TP0207`).
- The name exists only in the branch that runs when the test is true. In an `elif`, that's the `elif`'s own branch.
- In `while x is some v:`, `x` is tested again before each pass, and `v` holds the value for that pass.
- When `x` is a name or path that can be narrowed (below), `is some` narrows `x` itself too, as `x is not None` would.

```taipan
var queue = ["a", "b"]

def take() -> str?:
    if len(queue) == 0:
        return None
    return queue.pop(0)

while take() is some item:
    print("got", item)
```

```output
got a
got b
```

### `is some` must be the whole condition

`is some` can't be combined with `and` or `or`, because the name it introduces would be unclear in the rest of the expression. Doing so is error `TP0100`, and the help says what to write instead:

```taipan
def check(port: int?):
    if port is some p and p > 1024:
        print(p)

check(8080)
```

```error
main.tpn:2:23: error TP0100: 'is some' must be the whole condition
  help: nest another 'if', or use 'port is not None and …', which narrows 'port'
```

Use `is not None`, which narrows `port` for the rest of the condition, or nest a second `if`:

```taipan
def check(port: int?):
    if port is not None and port > 1024:
        print("high", port)
    if port is some p:
        if p > 1024:
            print("still high", p)

check(8080)
```

```output
high 8080
still high 8080
```

When the tested value is something that can't be narrowed, such as a call's result, the help suggests a `let` instead:

```taipan
def lookup(name: str) -> int?:
    return None

if lookup("db") is some p and p > 0:
    print(p)
```

```error
main.tpn:4:27: error TP0100: 'is some' must be the whole condition
  help: nest another 'if', or bind 'lookup("db")' with 'let' first
```

`is some` on a value that isn't optional is error `TP0200`:

```taipan
let n = 5
if n is some v:
    print(v)
```

```error
main.tpn:2:4: error TP0200: 'is some' needs an optional, found int
```

## `??`

`x ?? d` gives the value inside `x`, or `d` when `x` is `None`. `x` must be optional (`TP0200` otherwise). `x` is evaluated first, and `d` only when `x` is `None`.

With `x: T?`, `d` must be a `T`, and the result is a `T`. If `d` is itself a `T?`, the result is a `T?`, so several `??` can be chained, each trying the next:

```taipan
def fallback() -> int:
    print("computing the fallback")
    return 0

let configured: int? = None
let from_env: int? = 8080
print(configured ?? from_env ?? 80)
print(from_env ?? fallback())
print(configured ?? fallback())
```

```output
8080
8080
computing the fallback
0
```

`??` binds more loosely than every other operator, so `a ?? b + 1` is `a ?? (b + 1)`. Because that's easy to misread, an arithmetic operator (`+ - * / // % **`) directly on the right of `??` is warning `TP0305`. Parentheses on either side remove the warning:

```taipan
let counts: dict[str, int] = {}
let n = counts.get("a") ?? 0 + 1
print(n)
print((counts.get("a") ?? 0) + 1)
```

```output
1
1
```

```error
main.tpn:2:25: warning TP0305: '??' binds more loosely than '+', so this is 'counts.get("a") ?? (0 + 1)'
  help: write '(counts.get("a") ?? 0) + 1' if you meant that, or add the parentheses to keep this
```

## Narrowing

After a check, the compiler knows more about a value than its declared type says, and lets the code that runs only after that check use it as the narrower type. These conditions narrow:

| Condition | Narrows when it's true | Narrows when it's false |
|---|---|---|
| `x is not None`, `x != None` | `x` from `T?` to `T` | |
| `x is None`, `x == None` | | `x` from `T?` to `T` |
| `x is some v` | `x` from `T?` to `T`, and binds `v` | |
| `x == lit`, with `lit` an `int`, `float`, `str` or `bool` literal | `x` from `T?` to `T` | |
| `x != lit` | | `x` from `T?` to `T` |
| `isinstance(x, C)` | `x` to the class `C`, and rules out `None` | |

They combine:

- `a and b` is true when both are, so when it's true it narrows by both. In `b`, what `a` narrows already holds.
- `a or b` is false when both are, so when it's false it narrows by what both narrow when false. In `b`, what holds when `a` is false already holds, so `x is None or x > 0` is valid.
- `not a` swaps the two columns.
- Parentheses change nothing.

```taipan
def show(x: int?, y: int?):
    if x is None or y is None:
        print("missing")
    else:
        print(x + y)
    if not (x is None) and x > 0:
        print("positive", x)

show(1, 2)
show(None, 2)
```

```output
3
positive 1
missing
```

### Where narrowing applies

- **`if`, `elif` and `else`.** The `if` block sees what holds when its condition is true. Each later `elif` condition and block, and the `else` block, see what holds when the earlier conditions were false.
- **After an `if`.** The code after an `if` statement sees what holds at the end of every branch that can finish normally. A branch that ends in `return`, `raise`, `break` or `continue` doesn't count, and a missing `else` counts as a branch where every condition was false. So after `if x is None: return`, `x` is narrowed.
- **After a `match`**, likewise: what holds at the end of every arm that can finish normally.
- **`while c:`.** The body sees what holds when `c` is true. After the loop, if the body has no `break`, the code sees what holds when `c` is false.
- **`assert c`** narrows the code after it as if that code were inside `if c:`.
- **Loops.** A loop's body, and a `while` condition, run more than once, so they see what held before the loop, minus whatever the body could undo (an assignment, below). The same holds after a `for` loop, which may run zero times.
- **`try`.** The `except`, `else` and `finally` blocks, and the code after the `try`, see what held before it, minus whatever any of its blocks could undo. Narrowing found inside the `try` block doesn't reach them, since an exception could have come from any line of it.

```taipan
def first_port(ports: list[int?]) -> int:
    for p in ports:
        if p is None:
            continue
        return p + 1
    return 0

def label(name: str?) -> str:
    assert name is not None, "a name is required"
    return name.upper()

print(first_port([None, 8080]))
print(label("db"))
```

```output
8081
DB
```

### What can be narrowed

| Kind | Example | Narrowed until |
|---|---|---|
| a name that can't change: a `let`, a parameter (including `self`), a loop variable, a `match` or `is some` binding | `port` | never undone |
| a `var` | `count` | it's assigned |
| a path of fields | `self.config.port`, `p.x` | something could change it (below) |
| an element with a fixed key: an `int` or `str` literal, or a name that can't change | `xs[0]`, `row["id"]`, `xs[i]` | the path or the list or dict could change |

Anything else, such as a call's result or `xs[i]` with a `var` `i`, can't be narrowed. Copy it to a `let` first, or use `is some`.

```taipan
def lookup(name: str) -> int?:
    return 5432

if lookup("db") is not None:
    print(lookup("db") + 1)
```

```error
main.tpn:5:11: error TP0203: int? must be narrowed before use
  help: a call's result isn't narrowed, since calling it again may give another value; copy it to a 'let' first
```

```taipan
def lookup(name: str) -> int?:
    return 5432

let port = lookup("db")
if port is not None:
    print(port + 1)
```

```output
5433
```

### `var`s

A `var` stays narrowed until it's assigned. Assigning it a value of a narrower type narrows it to that type, so a `var` of type `int?` that has just been given an `int` is an `int` until the next assignment. This includes the assignment in its declaration, unpacking, and compound assignments such as `+=`.

```taipan
var port: int? = None
port = 8080
print(port + 1)
```

```output
8081
```

Assigning a value that may be `None` ends the narrowing, and the error says where:

```taipan
def lookup() -> int?:
    return None

var port: int? = 8080
print(port + 1)
port = lookup()
print(port + 1)
```

```error
main.tpn:7:7: error TP0203: int? must be narrowed before use
  help: it can be None again after the assignment on line 6
```

`is None`, `is not None`, `is some` and `??` see a narrowed name as its declared type, so testing a `var` that has been given a value isn't an error.

If a nested function assigns a `var`, that `var` isn't narrowed from the function's `def` on, nor anywhere in a loop that contains it, since any call could run the function and change it.

### Paths

A path of fields, `p.x` or `self.config.port`, is narrowed like a name, as long as its root is a name. Since another part of the program could change a field, the narrowing ends at anything that could:

- a call that could run Taipan or foreign code: a Taipan function, method or constructor, a function value, `super()`, any operation on a Python value, any .NET member, and the built-ins that take a function (`map`, `filter`, and `sorted` or `sort` with a `key`). Other built-ins, such as `len`, `print`, `str` and the list and dict methods, don't end it.
- anything that runs a special method or property of a class: reading or setting a property, or an operator, `==`, `in`, `print`, `str()`, `repr()`, `hash()`, an f-string hole or a dict access on a value whose class defines the special method used.
- an assignment to a field with the same name as one of the path's fields, on any object, since another name could refer to the same object.

Ending follows the order the code runs in: in `p.x is not None and check(p)`, the call ends the narrowing of `p.x` before the `if` block.

```taipan
class Config:
    port: int?

    def __init__(self, port: int?):
        self.port = port

def log(message: str):
    print(message)

def next_port(config: Config) -> int:
    if config.port is None:
        return 0
    log("checking")
    return config.port + 1

print(next_port(Config(8080)))
```

```error
main.tpn:14:12: error TP0203: int? must be narrowed before use
  help: the call on line 13 could have changed it; copy it to a 'let' first
```

Copying the field to a `let` before the call keeps the narrowed value:

```taipan
class Config:
    port: int?

    def __init__(self, port: int?):
        self.port = port

def log(message: str):
    print(message)

def next_port(config: Config) -> int:
    let port = config.port
    if port is None:
        return 0
    log("checking")
    return port + 1

print(next_port(Config(8080)))
```

```output
checking
8081
```

An element's narrowing also ends at any change to a list or dict of the same type, through any name: assigning or deleting an element, slice assignment, and the methods that change one (`append`, `extend`, `insert`, `pop`, `remove`, `clear`, `sort`, `reverse`, and a dict's `pop`, `setdefault`, `update` and `clear`). A tuple can't change, so its elements stay narrowed as long as the tuple's name does.

```taipan
let xs: list[int?] = [1, None]
if xs[0] is not None:
    print(xs[0] + 1)
```

```output
2
```

### Lambdas and nested functions

A lambda or nested function written where a value is narrowed sees the narrowing of `let`s, parameters and bindings, since they can't change. It doesn't see the narrowing of `var`s or paths, since it may run later, after they've changed:

```taipan
def scaled(factor: int?) -> list[int]:
    if factor is None:
        return []
    return list(map(lambda x: x * factor, [1, 2, 3]))

print(scaled(10))
```

```output
[10, 20, 30]
```

```taipan
var factor: int? = None
factor = 10
let scale: Callable[[int], int] = lambda x: x * factor
print(scale(2))
```

```error
main.tpn:3:49: error TP0203: int? must be narrowed before use
  help: a lambda or nested function runs later, when it may have changed; copy it to a 'let' first
```

### `isinstance`

`isinstance(x, C)` narrows `x` to the class `C` where it's true. On an `x` of type `A?`, it also rules out `None`, since `None` isn't an instance of any class. Testing against `x`'s own class, or a class it derives from, rules out `None` and nothing more.

```taipan
class Animal:
    name: str

    def __init__(self, name: str):
        self.name = name

class Dog(Animal):
    def bark(self) -> str:
        return self.name + " says woof"

def speak(a: Animal?):
    if isinstance(a, Dog):
        print(a.bark())
    elif a is not None:
        print(a.name, "is quiet")

speak(Dog("rex"))
speak(Animal("cat"))
speak(None)
```

```output
rex says woof
cat is quiet
```
