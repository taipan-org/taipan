---
title: Functions
description: "def, parameters, defaults, keyword arguments, *args and **kwargs, return, lambdas, functions as values, and nested functions."
sidebar:
  order: 5
---

A function is a named piece of code that takes values, called arguments, and may give back a value, called its result. This page covers declaring and calling functions, every kind of parameter, lambdas, using a function as a value, and functions inside functions. The rules are defined in [§4.2](../../reference/specification/#42-functions) and [§3.7](../../reference/specification/#37-functions-as-values-m11) of the specification.

## Declaring a function: `def`

`def` declares a function: its name, its parameters in parentheses, the type of its result after `->`, a colon, and an indented body.

```taipan
def area(width: float, height: float) -> float:
    return width * height

print(area(3.0, 4.5))
print(area(2, 5))
```

```output
13.5
10.0
```

- Every parameter needs a type, written after a colon. There's no default type, and the compiler never guesses one.
- `-> float` says what the function gives back. A function without `->` gives back nothing, and so does one declared `-> None`; the two mean the same.
- An `int` argument is accepted where a `float` is expected, as with `area(2, 5)`. No other conversion happens on its own.
- Top-level functions can be used before the line that declares them, so functions may call each other in any order, and a function may call itself.

```taipan
def is_even(n: int) -> bool:
    if n == 0:
        return True
    return is_odd(n - 1)

def is_odd(n: int) -> bool:
    if n == 0:
        return False
    return is_even(n - 1)

def factorial(n: int) -> int:
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print(is_even(10), is_odd(7), factorial(10))
```

```output
True True 3628800
```

A function name can be declared only once in a file. There's no overloading, where two functions share a name and differ in their parameters:

```taipan
def show(n: int) -> str:
    return str(n)

def show(s: str) -> str:
    return s
```

```error
main.tpn:4:5: error TP0214: duplicate definition of 'show'
```

## `return`

`return value` ends the function and gives `value` as its result. A function without a result type may use a bare `return` to end early.

```taipan
def sign(n: int) -> str:
    if n < 0:
        return "negative"
    if n == 0:
        return "zero"
    return "positive"

def greet(name: str):
    if name == "":
        return
    print(f"hello, {name}")

print(sign(-3), sign(0), sign(8))
greet("")
greet("ada")
```

```output
negative zero positive
hello, ada
```

`return a, b` gives back a tuple, which the caller can unpack:

```taipan
def min_max(xs: list[int]) -> tuple[int, int]:
    return min(xs), max(xs)

let lo, hi = min_max([4, 1, 9, 3])
print(lo, hi)
```

```output
1 9
```

Every path through a function with a result type must end in a `return` (or a `raise`). The compiler checks this. A loop doesn't count as ending the function, since it may finish, except a `while True:` with no `break`, which only a `return` or a `raise` can leave:

```taipan
def sign(n: int) -> str:
    if n < 0:
        return "negative"
    elif n > 0:
        return "positive"
```

```error
main.tpn:1:5: error TP0208: not all paths in 'sign' return a value
```

Returning a value from a function without a result type, or using its call as a value, is an error:

```taipan
def log(message: str):
    print(message)

let x = log("hi")
```

```error
main.tpn:4:9: error TP0200: 'log' does not return a value
```

## Calling a function

Arguments are matched to parameters in this order:

1. Positional arguments, written without a name, fill the parameters from the left.
2. Keyword arguments, written `name=value`, fill the parameter of that name, in any order.
3. Any parameter still unfilled takes its default value (below).

Keyword arguments come after positional ones. Arguments are evaluated left to right, as written, and then the missing defaults, in parameter order.

```taipan
def describe(name: str, age: int, city: str) -> str:
    return f"{name}, {age}, from {city}"

print(describe("Ada", 36, "London"))
print(describe("Ada", city="London", age=36))
print(describe(city="Paris", name="Blaise", age=39))
```

```output
Ada, 36, from London
Ada, 36, from London
Blaise, 39, from Paris
```

Every mistake in a call is a compile error: too many arguments, a missing one, one given twice, or a keyword that names no parameter.

```taipan
def describe(name: str, age: int, city: str) -> str:
    return f"{name}, {age}, from {city}"

print(describe("Ada", 36))
print(describe("Ada", 36, "London", "UK"))
print(describe("Ada", 36, name="Ada", city="London"))
print(describe("Ada", 36, town="London"))
```

```error
main.tpn:4:7: error TP0209: 'describe' takes 3 arguments, found 2
main.tpn:5:7: error TP0209: 'describe' takes 3 arguments, found 4
main.tpn:6:27: error TP0209: argument 'name' is given more than once
main.tpn:7:27: error TP0218: 'describe' has no parameter 'town'
```

A positional argument after a keyword argument is a syntax error:

```taipan
def describe(name: str, age: int) -> str:
    return f"{name}, {age}"

print(describe(name="Ada", 36))
```

```error
main.tpn:4:28: error TP0100: positional argument follows keyword argument
```

## Parameters can't be reassigned

A parameter is fixed for the whole call, like a `let`. To compute a new value, declare a variable with another name:

```taipan
def countdown(n: int):
    n = n - 1
```

```error
main.tpn:2:5: error TP0202: cannot assign to 'n': it is a parameter
```

```taipan
def countdown(n: int):
    var i = n
    while i > 0:
        print(i)
        i -= 1

countdown(3)
```

```output
3
2
1
```

A parameter that holds a list or dict can still have its contents changed, since that changes the object, not the parameter. The caller sees the change:

```taipan
def add_item(items: list[str], item: str):
    items.append(item)

let shopping = ["eggs"]
add_item(shopping, "milk")
print(shopping)
```

```output
["eggs", "milk"]
```

## Default values

A parameter may have a default value, written `= value` after its type. A call may then leave it out:

```taipan
def connect(host: str, port: int = 8080, secure: bool = False) -> str:
    var scheme = "http"
    if secure:
        scheme = "https"
    return f"{scheme}://{host}:{port}"

print(connect("example.com"))
print(connect("example.com", 443, True))
print(connect("example.com", secure=True))
```

```output
http://example.com:8080
https://example.com:443
https://example.com:8080
```

**A default is evaluated again on every call that leaves it out.** So a default of `[]` gives each call a new, empty list:

```taipan
def tagged(item: str, tags: list[str] = []) -> list[str]:
    tags.append(item)
    return tags

print(tagged("a"))
print(tagged("b"))
print(tagged("c", ["x"]))
```

```output
["a"]
["b"]
["x", "c"]
```

(This is one of Taipan's [differences from Python](../../reference/python-differences/): Python evaluates a default once, when the function is defined, and shares it between calls.)

The rules for defaults:

- The default is checked against the parameter's type, so `tags: list[str] = []` knows the list holds strings.
- Parameters with defaults come after those without, except for keyword-only parameters (below).
- A default may use constants, other functions and built-ins, but not the function's other parameters, and not a top-level `let` or `var`.

```taipan
def greet(greeting: str = "hello", name: str) -> str:
    return f"{greeting}, {name}"
```

```error
main.tpn:1:36: error TP0100: parameter without a default follows parameter with a default
```

```taipan
def pad(s: str, width: int, fill: str = " " * width) -> str:
    return s
```

```error
main.tpn:1:47: error TP0201: undefined name 'width'
```

## Positional-only and keyword-only parameters: `/` and `*`

Two markers in a parameter list restrict how arguments may be passed:

- Parameters **before `/`** are positional-only: they can't be passed by keyword.
- Parameters **after `*`** are keyword-only: they must be passed by keyword. Keyword-only parameters may have defaults or not, in any order.

```taipan
def clamp(value: float, /, *, low: float = 0.0, high: float) -> float:
    return max(low, min(value, high))

print(clamp(15.0, high=10.0))
print(clamp(-2.0, low=-1.0, high=1.0))
```

```output
10.0
-1.0
```

```taipan
def clamp(value: float, /, *, low: float = 0.0, high: float) -> float:
    return max(low, min(value, high))

print(clamp(value=15.0, high=10.0))
print(clamp(15.0, 0.0, 10.0))
print(clamp(15.0))
```

```error
main.tpn:4:13: error TP0218: 'value' is positional-only, so it can't be passed by keyword
main.tpn:5:7: error TP0209: 'clamp' takes 1 positional argument, found 3
main.tpn:6:7: error TP0209: missing keyword-only argument 'high'
```

The full order of a parameter list is: positional-only parameters and `/`, then ordinary parameters, then `*` (or `*args`) and keyword-only parameters, then `**kwargs`. A `*` must be followed by at least one keyword-only parameter. Anything out of this order is a syntax error:

```taipan
def f(a: int, *, /):
    pass
```

```error
main.tpn:1:18: error TP0100: '/' must come before '*'
```

## Any number of arguments: `*args` and `**kwargs`

`*name: T` collects the positional arguments left over after the other positional parameters are filled, in order, into a new `list[T]`. `**name: T` collects the keyword arguments that name no other parameter into a new `dict[str, T]`, in the order they were written. Either is empty when there's nothing to collect. Note that each is annotated with the type of one element, not the whole list or dict.

```taipan
def total(label: str, *amounts: float, **adjustments: float) -> str:
    var sum = 0.0
    for a in amounts:
        sum += a
    for name, value in adjustments.items():
        print(f"  {name}: {value}")
        sum += value
    return f"{label}: {sum}"

print(total("none"))
print(total("groceries", 12.5, 3.25, 4))
print(total("rent", 900, discount=-50, fee=10))
```

```output
none: 0.0
groceries: 19.75
  discount: -50.0
  fee: 10.0
rent: 860.0
```

The names `args` and `kwargs` are only a convention; any name will do. Parameters after `*args` are keyword-only, just as after a bare `*`.

### Unpacking arguments at a call

At a call, `*value` spreads a tuple or a list into positional arguments, and `**value` spreads a `dict[str, T]` into keyword arguments:

```taipan
def point(x: int, y: int) -> str:
    return f"({x}, {y})"

def joined(*words: str) -> str:
    return " ".join(words)

def settings(**options: int) -> str:
    return str(options)

let pair = (3, 4)
let words = ["unpacked", "into", "args"]
let opts = {"width": 80, "depth": 3}
print(point(*pair))
print(joined("now", *words))
print(settings(**opts, height=24))
```

```output
(3, 4)
now unpacked into args
{"width": 80, "depth": 3, "height": 24}
```

- A tuple's length is known, so its elements can fill any positional parameters.
- A list's length isn't known until the program runs, so every element of a `*list` must go to `*args`: the parameters before it must already be filled.
- Every entry of a `**dict` goes to `**kwargs`, so the function needs one.

```taipan
def point(x: int, y: int) -> str:
    return f"({x}, {y})"

let coords = [3, 4]
print(point(*coords))
```

```error
main.tpn:5:13: error TP0209: 'point' has no '*args' for '*coords'
```

## Lambdas

A `lambda` is a small function without a name, written as an expression: `lambda` then its parameters, a colon, and one expression, whose value is the result.

```taipan
let words = ["banana", "kiwi", "apple", "fig"]
print(sorted(words, key=lambda w: len(w)))
print(sorted(words, key=lambda w: w[-1]))

let add = lambda: 1 + 2
print(add())
```

```output
["fig", "kiwi", "apple", "banana"]
["banana", "apple", "fig", "kiwi"]
3
```

- A lambda's parameters have no type annotations. Their types come from where the lambda is used: here, `sorted`'s `key` expects a function taking a `str`.
- A lambda with no parameters, like `add` above, takes its body's type when nothing else gives one.
- A lambda with parameters and nothing to give their types is an error. Give the variable a function type (below):

```taipan
let double = lambda x: x * 2
```

```error
main.tpn:1:21: error TP0206: cannot infer the type of lambda parameter 'x'
  help: add a type annotation
```

- A lambda's parameters can't have defaults:

```taipan
let fs = [lambda i=0: i]
```

```error
main.tpn:1:19: error TP0100: lambda parameters can't have defaults
  help: each loop iteration has its own variable, so 'lambda: i' already sees this iteration's value
```

- The body is one expression. For anything longer, use a nested `def` (below).

## Functions as values and `Callable`

A function can be stored in a variable, passed to another function, kept in a list or a field, and called later. Its type is written `Callable[[parameter types], result type]`:

| Type | A function that |
|---|---|
| `Callable[[int, str], bool]` | takes an `int` and a `str`, and returns a `bool` |
| `Callable[[], int]` | takes nothing and returns an `int` |
| `Callable[[str], None]` | takes a `str` and returns nothing |

`Callable` is built in, like `list`, so it needs no import. These are function values:

- the name of a top-level function, without calling it (`apply(double, 5)`)
- a `lambda`
- a nested `def` (below)
- a class's name, which is its constructor, and an enum variant with fields
- a built-in function or type, such as `len` or `str`, or a generic function, when the place it's used says what function type it must have, as with `size` below

```taipan
def double(n: int) -> int:
    return n * 2

def apply(f: Callable[[int], int], x: int) -> int:
    return f(x)

let steps: list[Callable[[int], int]] = [double, lambda n: n + 1, abs]
var value = -5
for step in steps:
    value = apply(step, value)
print(value)
print(list(map(str, [1, 2, 3])))
print(double)
let size: Callable[[str], int] = len
print(size, size("four"))
```

```output
9
["1", "2", "3"]
<function double>
<built-in function len> 4
```

The rules for function values:

- A function type has no parameter names or defaults, so a call through a function value passes every argument by position. A keyword argument is an error.
- A function with `*args` or `**kwargs` can't be used as a value. Wrap it in a lambda.
- A method can't be used as a value either: write `lambda s: s.total()` instead.
- A function that accepts a more general parameter type, or returns a more specific result type, fits a function type: a `Callable[[Animal], Dog]` can be used where a `Callable[[Dog], Animal]` is expected.
- Function values compare by identity: a top-level function is always equal to itself, and each evaluation of a `lambda` makes a new value. `<` and the other orderings don't apply to functions.

```taipan
def apply(f: Callable[[int], int], x: int) -> int:
    return f(n=x)
```

```error
main.tpn:2:14: error TP0218: a function value takes no keyword arguments
```

```taipan
class Counter:
    n: int = 0

    def bump(self) -> int:
        self.n += 1
        return self.n

let c = Counter()
let f: Callable[[], int] = c.bump
```

```error
main.tpn:9:30: error TP0200: 'bump' is a method; call it
  help: to use it as a function, wrap it in a lambda
```

## Nested functions

A `def` inside another function, or inside any block, declares a function local to that block. Its name can be used from the `def` to the end of the block, and inside its own body, so it may call itself.

```taipan
def report(scores: list[int]) -> list[str]:
    def grade(score: int) -> str:
        if score >= 50:
            return "pass"
        return "fail"
    return [f"{s}: {grade(s)}" for s in scores]

print(report([72, 40, 50]))
```

```output
["72: pass", "40: fail", "50: pass"]
```

- Unlike a top-level function, a nested function can't be used before its `def`.
- A nested function can't have type parameters of its own, but it may use those of the function around it.
- `return`, `break` and `continue` inside it belong to the nested function, not to a loop around it.

### Capturing variables

A lambda or nested function may use any variable visible where it's written: the enclosing function's parameters and variables, and the main file's top-level variables. This is called capturing.

- A captured `var` is shared, not copied: the function sees later changes to it.
- A `let`, a parameter or a loop variable can't change, so capturing one captures its value.
- **Each loop iteration has its own loop variable**, so functions made in a loop each see the value from their own iteration:

```taipan
var fs: list[Callable[[], int]] = []
for i in range(3):
    fs.append(lambda: i * 10)
print([f() for f in fs])

var count = 0
let show = lambda: print("count is", count)
count = 5
show()
```

```output
[0, 10, 20]
count is 5
```

### Assigning to an enclosing function's variable: `nonlocal`

A nested function may read a `var` of the function around it freely, but to assign to it, it must first declare the name with `nonlocal`:

```taipan
def make_counter() -> Callable[[], int]:
    var count = 0
    def next_count() -> int:
        nonlocal count
        count += 1
        return count
    return next_count

let counter = make_counter()
counter()
counter()
print(counter())
```

```output
3
```

Without `nonlocal`, the assignment is an error, and the help says what to write:

```taipan
def make_counter() -> Callable[[], int]:
    var count = 0
    def next_count() -> int:
        count += 1
        return count
    return next_count
```

```error
main.tpn:4:9: error TP0236: 'count' belongs to an enclosing function, so assigning to it needs 'nonlocal'
  help: add 'nonlocal count' at the start of this function, or use 'let' with another name for a new variable
```

The rules for `nonlocal`:

- `nonlocal` lines come first in the nested function's body (after a docstring, if it has one). One line may list several names: `nonlocal a, b`.
- The name must be a variable of an enclosing function, not one the nested function declares itself.
- It doesn't make a `let`, a parameter or a loop variable assignable: those still can't change.
- It's only for nested functions. No function can assign to a top-level variable of the main file, with or without `nonlocal`: pass the value in and return the new one instead. Changing what a top-level variable holds, as in `items.append(x)`, is allowed.

```taipan
var total = 0

def add(n: int):
    total += n
```

```error
main.tpn:4:5: error TP0236: a function can't assign to the top-level variable 'total'
  help: pass it in and return the new value
```

```taipan
def outer():
    nonlocal x
```

```error
main.tpn:2:5: error TP0236: 'nonlocal' can only be used in a nested function
```

## Docstrings

A string on the first line of a function's body documents the function. It does nothing when the function runs.

```taipan
def area(width: float, height: float) -> float:
    """The area of a width by height rectangle."""
    return width * height

print(area(2.0, 3.0))
```

```output
6.0
```

## Decorators

A decorator is a line starting with `@` before a `def` or `class`. A top-level function takes no decorators. The ones Taipan has apply to classes and methods, and are covered in [Classes](../classes/): `@dataclass`, `@override`, `@property` and `@<name>.setter`, `@staticmethod`, `@classmethod`, `@abstractmethod`, `@final` and `@total_ordering`. Any other decorator, or one in the wrong place, is an error:

```taipan
@cache
def fib(n: int) -> int:
    if n < 2:
        return n
    return fib(n - 1) + fib(n - 2)
```

```error
main.tpn:1:1: error TP0224: unknown decorator '@cache'
```

## Related pages

- `async def` declares a function that can wait without blocking: [async and await](../async/).
- A function containing `yield` is a generator: [Iterators and generators](../generators/).
- Generic functions, which take type parameters such as `def first[T](xs: list[T]) -> T?`, are covered in [Types](../types/#generics).
- Methods, functions that belong to a class, are covered in [Classes](../classes/).
