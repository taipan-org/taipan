---
title: Variables and constants
description: "let, var and const, type annotations and inference, assignment and compound assignment, unpacking, scope, top-level names, and del."
sidebar:
  order: 2
---

A *binding* gives a name to a value. Taipan has three kinds, and every name has to be declared with one of them before it's used:

| Keyword | Can be reassigned | Where | Its value |
|---|---|---|---|
| `let` | no | anywhere | any expression |
| `var` | yes | anywhere | any expression |
| `const` | no | the top level of a file only | a constant expression, built from literals |

Bindings are defined in [§4.1](../../reference/specification/#41-bindings), with the checking rules in [§4.5](../../reference/specification/#45-names-and-checking-rules).

## `let`

`let name = value` declares a name whose value can't be replaced. Use it for most names: a reader then knows the name means the same thing everywhere it appears.

```taipan
let name = "api"
let replicas = 3
print(name, replicas)
```

```output
api 3
```

Assigning to a `let` binding again is an error, whose help says how to make it a variable instead:

```taipan
let name = "api"
name = "web"
```

```error
main.tpn:2:1: error TP0202: cannot assign to 'name': it is a let binding
  help: declare it with 'var'
```

A `let` binding can't be pointed at a different value, but the value itself may still change if it's a kind of value that can. A `let` holding a list can have elements appended or replaced:

```taipan
let xs = [1, 2]
xs.append(3)
xs[0] = 10
print(xs)
```

```output
[10, 2, 3]
```

## `var`

`var name = value` declares a variable, which may be assigned a new value later. The new value must have the variable's type.

```taipan
var count = 0
count = count + 1
count = 10
print(count)
```

```output
10
```

```taipan
var total = 0
total = "zero"
```

```error
main.tpn:2:9: error TP0200: expected int, found str
```

## `const`

`const NAME = value` declares a constant. Constants are written only at the top level of a file, not inside a function or a block, and they're visible everywhere in the file, including inside functions declared before them. By convention their names are in capitals.

The value must be a *constant expression*: literals (numbers, strings, `True`, `False`), combined only with unary `-` and the operators `+ - * / // %`. A constant can't refer to another constant or call a function.

```taipan
def describe() -> str:
    return f"{NAME} allows {LIMIT} requests"

const NAME = "api"
const LIMIT = 60 * 24
print(describe())
```

```output
api allows 1440 requests
```

```taipan
const A = 1
const B = A + 1
```

```error
main.tpn:2:11: error TP0217: const initializer must be a constant expression
```

A `const` inside a function is a syntax error:

```taipan
def f() -> None:
    const C = 1
```

```error
main.tpn:2:5: error TP0100: 'const' can only be at the top level
  help: use 'let' here
```

The value is computed when the program starts, so `const X = 1 // 0` compiles but stops the program with a `ZeroDivisionError` as soon as it runs.

## Types and inference

Every binding has a type, fixed when the program compiles. Without an annotation, the type is *inferred*: it's the type of the value the binding starts with. To give a type yourself, write it after the name, with a colon:

```taipan
let ratio = 0.5
let limit: float = 10
var names: list[str] = []
let nickname: str? = None
print(ratio, limit, names, nickname)
```

```output
0.5 10.0 [] None
```

An annotation is needed in these cases:
- **The value doesn't say enough.** An empty list `[]`, an empty dict `{}`, and `None` alone have no type to infer.
- **You want a wider type than the value has.** An `int` is accepted where a `float` is expected, and turns into one, so `let limit: float = 10` holds `10.0`. A plain value is accepted where an optional (`T?`) is expected, so `var current: str? = "a"` may later be set to `None`. See [Optionals](../optionals/).

```taipan
let xs = []
```

```error
main.tpn:1:10: error TP0206: cannot infer the type of an empty list
  help: add a type annotation
```

```taipan
let missing = None
```

```error
main.tpn:1:15: error TP0206: cannot infer the type of None
  help: add a type annotation
```

A value that doesn't fit the annotation is an error:

```taipan
let n: int = "three"
```

```error
main.tpn:1:14: error TP0200: expected int, found str
```

The rules for which types are accepted where are in [§3.3](../../reference/specification/#33-assignability), and inference in [§3.2](../../reference/specification/#32-inference).

## Assignment

`name = value` gives a `var` a new value. Assignment also works on an element of a list or dict, `xs[i] = value` and `d[key] = value`, and on a field of an object, `obj.field = value`. Those change the list, dict or object, so they work whether the list or object is held by a `let` or a `var`.

Only a `var` can be assigned. Assigning to a `let`, a function's parameter, a loop variable, a constant or a function is error `TP0202`:

```taipan
for i in range(3):
    i = 5
```

```error
main.tpn:2:5: error TP0202: cannot assign to 'i': it is a loop variable
```

```taipan
const LIMIT = 3
LIMIT = 4
```

```error
main.tpn:2:1: error TP0202: cannot assign to 'LIMIT': it is a constant
```

An element of a string or a tuple can't be assigned either, since those can't change:

```taipan
let s = "abc"
s[0] = "x"
```

```error
main.tpn:2:1: error TP0202: cannot assign to an element of str
```

Assignment is a statement, not an expression, so it can't appear inside a condition, an argument or another expression.

## Compound assignment

`x op= value` combines `x` with `value` and assigns the result back to `x`:

| Statement | Means |
|---|---|
| `x += y` | `x = x + y` |
| `x -= y` | `x = x - y` |
| `x *= y` | `x = x * y` |
| `x /= y` | `x = x / y` |
| `x //= y` | `x = x // y` |
| `x %= y` | `x = x % y` |
| `x **= y` | `x = x ** y` |

The target may be a `var`, an element (`counts[k] += 1`) or a field (`self.total += n`).

```taipan
var n = 10
n -= 3
n *= 4
n //= 3
n %= 5
n **= 3
print(n)
var s = "ab"
s += "c"
s *= 2
print(s)
```

```output
64
abcabc
```

Each one is checked as the longer form would be. So `n /= 2` on an `int` variable is an error, because `n / 2` is a `float`, and a `float` can't be stored in an `int` variable:

```taipan
var n = 1
n /= 2
```

```error
main.tpn:2:6: error TP0200: expected int, found float
```

**`+=` on a list** is the exception to "means `x = x + y`". It adds the elements of the right side to the list itself, so every name for that list sees them. Since the name isn't given a new value, the list may be held by a `let`, a parameter or a loop variable. The right side must be a list of the same element type.

**`*=` on a list and `-=`, `|=`, `&=` and `^=` on a set** are compile errors. In Python each changes the collection in place; checked as `x = x op y` they'd make a new one and leave other names for the collection unchanged, so instead the error says to write `xs = xs * n` or `s = s | t`, which make a new collection openly.

```taipan
let xs = [1]
let same = xs
xs += [2, 3]
print(xs, same)
```

```output
[1, 2, 3] [1, 2, 3]
```

`*=` on a list is an error. Write `xs = xs * n`, which makes a new list:

```taipan
var xs = [1]
xs *= 2
```

```error
main.tpn:2:1: error TP0200: '*=' isn't supported on a list
  help: write 'xs = xs * n', which makes a new list
```

There are no `&=`, `|=`, `^=`, `<<=` or `>>=` on numbers, since Taipan has no operators for working with the bits of an `int` (see [Operators](../operators/#operators-taipan-doesnt-have)).

## Unpacking

A `let`, `var` or assignment with several names separated by commas takes a tuple or a list apart, giving each name one element, in order. Tuples are described in the [tuple reference](../../library/tuple/), and unpacking in [§3.8](../../reference/specification/#38-tuples-m12).

```taipan
let lo, hi = (1, 9)
var a, b = 1, 2
a, b = b, a
print(lo, hi, a, b)
```

```output
1 9 2 1
```

Without parentheses, values separated by commas after `=` are a tuple, so `var a, b = 1, 2` unpacks the tuple `(1, 2)`. In an assignment, the whole right side is computed before anything is assigned, so `a, b = b, a` swaps the two values.

The rules:
- **Counting.** A tuple must have exactly as many elements as there are names, which the compiler checks. A list's length is only known when the program runs, so a list with the wrong number raises `ValueError` then.
- **`_` ignores an element.** It may appear any number of times.
- **`*name` takes the rest** as a new list. It may appear once at each level, anywhere among the names, and needs at least as many elements as the other names.
- **Parentheses nest.** `(a, b), c` takes apart a value whose first element is itself a pair.
- **No annotations.** Each name gets the type of its element, so `let a: int, b = …` is a syntax error.

```taipan
let first, *rest = [1, 2, 3, 4]
let head, *_, last = "a,b,c,d".split(",")
let (x, y), label = ((3, 4), "point")
let _, count = ("ignored", 7)
print(first, rest, head, last, x, y, label, count)
```

```output
1 [2, 3, 4] a d 3 4 point 7
```

```taipan
let a, b = (1, 2, 3)
```

```error
main.tpn:1:12: error TP0209: tuple[int, int, int] has 3 elements, found 2 names
```

```taipan
let a, b = [1, 2, 3]
```

```error
Traceback (most recent call last):
  File "main.tpn", line 1, in <module>
    let a, b = [1, 2, 3]
ValueError: too many values to unpack (expected 2)
```

A starred name can't be the only one, and a compound assignment can't unpack:

```taipan
var xs = [1, 2]
*rest = xs
```

```error
main.tpn:2:1: error TP0100: a starred name can't be unpacked to alone
  help: add another name, or drop the '*'
```

```taipan
var a = 1
var b = 2
a, b += 1
```

```error
main.tpn:3:6: error TP0100: expected '=', found '+='
```

`for` loops unpack their elements the same way: see [Control flow](../control-flow/#unpacking-in-a-for-loop).

## Scope

A name is visible from its declaration to the end of the block it's declared in. A *block* is the indented lines under an `if`, `elif`, `else`, `while`, `for`, `match` arm, `try`, `with` or `def`. When the block ends, its names are gone:

```taipan
let ok = True
if ok:
    let message = "hello"
    print(message)
print(message)
```

```error
main.tpn:5:7: error TP0201: undefined name 'message'
```

To use a value after the block, declare the name before it, with a `var`, and assign it inside:

```taipan
let ok = True
var message = ""
if ok:
    message = "hello"
print(message)
```

```output
hello
```

**Names are used after they're declared.** Using a name before its `let` or `var` is an error:

```taipan
print(total)
let total = 1
```

```error
main.tpn:1:7: error TP0201: undefined name 'total'
```

**No shadowing.** A name declared in an inner block can't have the same name as one visible from an outer block, function or the top level. That's error `TP0207`, so a name always means one thing within a function.

```taipan
let x = 1
if x > 0:
    let x = 2
```

```error
main.tpn:3:9: error TP0207: 'x' shadows an outer binding
```

**No redeclaring.** Declaring the same name twice in one block is error `TP0214`. To change a value, declare it with `var` and assign it.

```taipan
var x = 1
var x = 2
```

```error
main.tpn:2:5: error TP0214: duplicate definition of 'x'
```

The special cases:
- **Loop variables.** The variable of a `for` loop is a `let` that belongs to the loop's body, and each pass through the loop gets its own. So a function made inside the loop keeps the value from its own pass.
- **Comprehensions** have their own scope. Their variables are visible only inside the brackets, and may reuse a name from outside, which is left unchanged: see [Control flow](../control-flow/#comprehensions).
- **Built-in function names** such as `sum`, `max` or `len` may be reused for a local name: `var sum = 0`. Calling `sum(…)` after that is an error, since `sum` is then an `int`. Names that are also types (`int`, `float`, `str`, `list`, `set`) can't be reused.
- **Function parameters** are declared in the function's outermost block, and can't be reassigned, like `let`. A `let` in the body with a parameter's name is `TP0207`.

```taipan
let list = [1, 2]
```

```error
main.tpn:1:5: error TP0207: 'list' shadows an outer binding
```

## Top-level names and functions

A `let` or `var` written directly at the top level of the main file, not inside any block, can be read by every function and method in that file. A function sees the binding's current value when it runs, not the value it had when the function was declared:

```taipan
var count = 0
let names: list[str] = []

def add(name: str) -> None:
    names.append(name)
    print(f"{len(names)} names, count is {count}")

add("ana")
count = 5
add("ben")
```

```output
1 names, count is 0
2 names, count is 5
```

The rules:
- **Reading only.** A function can change what a top-level binding holds (`names.append(…)`, `table[key] = value`), but it can't assign the binding a new value, including with `+=`. Taipan has no `global` statement to allow it. Pass the value in as a parameter and return the new value instead.
- **The function's own names come first.** A parameter or local of a function may have the same name as a top-level binding, and then the function uses its own: `def find(names: list[str])` is fine beside a top-level `names`.
- **Set before use.** A top-level statement that calls a function before a binding the function reads has been set is an error, which names the line to move the call below.
- **Only direct top-level bindings.** A name declared inside a top-level `if`, `for` or other block, or by unpacking, can't be read by functions.
- **Not in defaults.** A parameter's default value can't use a top-level `let` or `var`. Use a `const`, or pass the value in.
- **The main file only.** Other files (modules) don't see the main file's bindings, and a module's top level holds only declarations (`def`, `class`, `enum`, `const`, imports), not `let` or `var`.

```taipan
var count = 0

def bump() -> None:
    count += 1
```

```error
main.tpn:4:5: error TP0236: a function can't assign to the top-level variable 'count'
  help: pass it in and return the new value
```

```taipan
def show() -> None:
    print(limit)

show()
let limit = 3
```

```error
main.tpn:4:1: error TP0201: 'show' reads 'limit', which isn't set until line 5
  help: move this below line 5
```

```taipan
let pairs = [(1, 2)]
for a, b in pairs:
    let total = a + b

def report() -> int:
    return total
```

```error
main.tpn:6:12: error TP0211: 'report' cannot use top-level variable 'total'
  help: it's bound inside a block or by unpacking; give it its own top-level 'let' or 'var'
```

```taipan
let width = 80

def pad(s: str, w: int = width) -> str:
    return s.ljust(w)
```

```error
main.tpn:3:26: error TP0211: 'pad' cannot use top-level variable 'width'
  help: pass it as a parameter, or make it a const
```

The help suggests `const` when the binding could be one, as `width` here can. With `const WIDTH = 80` it works.

## `del`

`del` removes elements from a list and entries from a dict. It's defined in [§5.5](../../reference/specification/#55-slice-assignment-and-del-m26).

| Statement | Removes |
|---|---|
| `del xs[i]` | the element at index `i`; a negative `i` counts from the end |
| `del xs[a:b]`, `del xs[a:b:step]` | every element in the slice (see [Operators](../operators/#slicing)) |
| `del d[key]` | the entry for `key`; the other entries keep their order |
| `del t1, t2, …` | each target in turn, left to right |

```taipan
var xs = [10, 20, 30, 40, 50]
var ages = {"ana": 31, "ben": 27}
del xs[0]
print(xs)
del xs[1:3]
print(xs)
del ages["ana"], xs[-1]
print(ages, xs)
```

```output
[20, 30, 40, 50]
[20, 50]
{"ben": 27} [20]
```

An index out of range raises `IndexError`, and a missing key raises `KeyError`. A slice never raises, since its bounds are clipped to the list's length, except that a step of `0` raises `ValueError`. Deleting from a list or dict while a loop goes over it raises `RuntimeError`.

```taipan
var ages = {"ana": 31}
del ages["zoe"]
```

```error
Traceback (most recent call last):
  File "main.tpn", line 2, in <module>
    del ages["zoe"]
KeyError: "zoe"
```

`del` only removes elements and entries. A name can't be deleted: give a `var` a new value instead. A field, or a character of a string or an element of a tuple, can't be deleted either.

```taipan
var x = 1
del x
```

```error
main.tpn:2:5: error TP0202: cannot delete 'x'
  help: assign it a new value instead
```
