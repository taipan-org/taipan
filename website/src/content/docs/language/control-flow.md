---
title: Control flow
description: "if, elif and else, conditions, while, for, break, continue, pass, and list, dict and set comprehensions."
sidebar:
  order: 3
---

Statements run one after another, from the top of the file down. The statements on this page choose which statements run, and how many times. Each one ends its first line with a colon, followed by a *block*: one or more lines indented further than that line. The block ends at the first line indented no further than the statement itself.

Conditions are defined in [§3.5](../../reference/specification/#35-conditions), loops in [§5.2](../../reference/specification/#52-loops), and comprehensions in [§3.16](../../reference/specification/#316-comprehensions-m36). [`match`](../match/), which chooses by the shape of a value, has its own page.

## Conditions are `bool`

The condition of an `if`, `elif` or `while`, and each operand of `and`, `or` and `not`, must be a `bool`: `True` or `False`. Nothing else stands in for one. A number, a string, a list or an optional isn't treated as true or false, and using one as a condition is error `TP0205`:

```taipan
let xs = [1, 2]
if xs:
    print("not empty")
```

```error
main.tpn:2:4: error TP0205: condition must be bool, found list[int]
```

```taipan
let name = ""
if not name:
    print("no name")
```

```error
main.tpn:2:8: error TP0205: condition must be bool, found str
```

Write the test you mean instead:

| To test | Write |
|---|---|
| a list, dict, set or string isn't empty | `len(xs) > 0` |
| a string is empty | `name == ""` |
| a number isn't zero | `n != 0` |
| an optional holds a value | `x is not None`, or `x is some v` (see [Optionals](../optionals/)) |

```taipan
let xs = [1, 2]
let name = ""
if len(xs) > 0 and name == "":
    print("a list, but no name")
```

```output
a list, but no name
```

## `if`, `elif` and `else`

`if` runs its block when its condition is `True`. Any number of `elif` clauses may follow, each with its own condition, and then one `else`, whose block runs when none of the conditions was `True`. The conditions are tested in order, and only the first block whose condition is `True` runs.

```taipan
let temperature = 15
if temperature < 10:
    print("cold")
elif temperature < 20:
    print("mild")
else:
    print("hot")
```

```output
mild
```

A condition may also be `x is some v`, which runs the block only when the optional `x` holds a value, and names that value `v` inside the block. A test such as `x is not None` *narrows* `x`: inside the block, `x` can be used as the value it holds. Both are described on [Optionals](../optionals/).

```taipan
let ages = {"ana": 31}
if ages.get("ana") is some age:
    print(f"ana is {age}")
```

```output
ana is 31
```

### No conditional expression

There's no `a if condition else b` that gives one of two values. To choose a value, use a [`match` expression](../match/#match-expressions) on a `bool`, or an `if` statement that assigns a `var`:

```taipan
let n = 3
let size = "big" if n > 10 else "small"
```

```error
main.tpn:2:18: error TP0100: Taipan has no conditional expression
  help: use a 'match' expression or an 'if' statement
```

```taipan
let n = 3
let size = match n > 10:
    True: "big"
    False: "small"
var parity = "odd"
if n % 2 == 0:
    parity = "even"
print(size, parity)
```

```output
small odd
```

## `while`

`while` tests its condition, runs its block if it's `True`, and repeats, until the condition is `False`. The condition is tested before each pass, so the block may not run at all.

```taipan
var n = 1
while n < 100:
    n = n * 3
print(n)
```

```output
243
```

`while True:` loops until a `break` (below) leaves it. `while x is some v:` tests the optional `x` again before each pass, and runs the block with `v` holding its value while it isn't `None`:

```taipan
var todo = ["c", "b", "a"]
var current: str? = todo.pop()
while current is some item:
    print(item)
    current = match len(todo):
        0: None
        _: todo.pop()
```

```output
a
b
c
```

## `for`

`for name in values:` runs its block once for each value in turn, with `name` holding the value. What a `for` loop can go over:

| Values | Each `name` is |
|---|---|
| a `list[T]` | each element, a `T`, in order |
| a `dict[K, V]` | each key, a `K`, in the order the keys were added |
| a `set[T]` | each element, a `T`, in the order they were added |
| a `str` | each character, as a one-character `str` |
| `range(stop)`, `range(start, stop)`, `range(start, stop, step)` | each `int` from `start` (default `0`) up to, but not including, `stop`, counting by `step` (default `1`) |
| an iterator, such as `enumerate(xs)`, `zip(xs, ys)`, `reversed(xs)` or a generator | each value it gives |

```taipan
for i in range(3):
    print(i)
for i in range(10, 0, -4):
    print(i)
for c in "hi":
    print(c)
let ages = {"ana": 31, "ben": 27}
for name in ages:
    print(name)
```

```output
0
1
2
10
6
2
h
i
ana
ben
```

A negative `step` counts down, as `range(10, 0, -4)` does, and a `step` of `0` raises `ValueError`. Anything else is an error:

```taipan
for x in 5:
    print(x)
```

```error
main.tpn:1:10: error TP0200: cannot iterate over int
```

A fixed-size tuple can't be looped over, since its elements may have different types. An optional can't either, until it's narrowed.

The loop variable is a `let` that belongs to the body: it can't be assigned, it isn't visible after the loop, and each pass gets a new one. So a function made in the body keeps that pass's value.

```taipan
var greeters: list[Callable[[], str]] = []
for name in ["ana", "ben"]:
    greeters.append(lambda: f"hi {name}")
for greet in greeters:
    print(greet())
```

```output
hi ana
hi ben
```

### Unpacking in a `for` loop

When each value is a tuple or a list, the loop can take it apart into several names, with the same rules as [unpacking a `let`](../variables/#unpacking): `_` ignores an element, `*rest` takes the rest as a list, and parentheses nest. `enumerate(xs)` gives `(index, element)` pairs, counting from `0` or from `start=`, and `d.items()` gives `(key, value)` pairs.

```taipan
for i, name in enumerate(["ana", "ben"], start=1):
    print(i, name)
let ages = {"ana": 31, "ben": 27}
for name, age in ages.items():
    print(f"{name} is {age}")
for i, (name, age) in enumerate(ages.items()):
    print(i, name, age)
for first, *rest in [[1, 2, 3], [4]]:
    print(first, rest)
```

```output
1 ana
2 ben
ana is 31
ben is 27
0 ana 31
1 ben 27
1 [2, 3]
4 []
```

### Changing a collection during a loop

Adding to or removing from a list, dict or set while a loop goes over it raises `RuntimeError` at the loop's next step. Replacing an element that's already there (`xs[i] = v`, or `d[k] = v` for a key `d` already has) is allowed, and later steps see the new value. To add while going over a list, loop over a copy: `for x in xs.copy():`.

```taipan
var xs = [1, 2, 3]
for x in xs:
    xs.append(x)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 2, in <module>
    for x in xs:
RuntimeError: list changed size during iteration
```

A `break` straight after the change leaves the loop before its next step, so it doesn't raise.

## `break` and `continue`

`break` leaves the innermost loop at once. `continue` skips the rest of the body, and goes on to the loop's next pass. Both apply to the loop they're written in, also from inside an `if` or a `match` arm, and either one outside a loop is error `TP0215`.

```taipan
for i in range(10):
    if i % 2 == 0:
        continue
    if i > 6:
        break
    print(i)
```

```output
1
3
5
```

```taipan
break
```

```error
main.tpn:1:1: error TP0215: 'break' outside a loop
```

A loop has no `else` clause. To do something only when a loop ends without `break`, set a `var` before the `break` and test it after the loop.

```taipan
for i in range(3):
    print(i)
else:
    print("done")
```

```error
main.tpn:3:1: error TP0100: expected statement, found 'else'
```

```taipan
let words = ["ant", "bee", "cat"]
var found = False
for w in words:
    if w.startswith("b"):
        found = True
        break
if not found:
    print("no b")
print(found)
```

```output
True
```

### Loops and `return`

A function that declares a return type must end every path with a `return` (or a `raise`). The compiler doesn't count a `for`, or a `while` with a condition, as ending a path, since the loop may finish, so a `return` is still needed after it:

```taipan
def first_even(xs: list[int]) -> int:
    for x in xs:
        if x % 2 == 0:
            return x
```

```error
main.tpn:1:5: error TP0208: not all paths in 'first_even' return a value
```

```taipan
def first_even(xs: list[int]) -> int:
    for x in xs:
        if x % 2 == 0:
            return x
    return -1

print(first_even([3, 8, 5]), first_even([1]))
```

```output
8 -1
```

A `while True:` with no `break` is the exception: only a `return` or a `raise` can leave it, so a loop that returns from inside needs nothing after it. Code after such a loop can't run, and gets warning `TP0300`.

```taipan
def first_long(words: list[str]) -> str:
    var i = 0
    while True:
        if len(words[i]) > 3:
            return words[i]
        i += 1

print(first_long(["a", "bb", "dddd", "eeeee"]))
```

```output
dddd
```

## `pass`

A block can't be empty. `pass` is a statement that does nothing, for a block that has nothing to do.

```taipan
for i in range(3):
    pass
print("done")
```

```output
done
```

```taipan
let n = 3
if n > 0:
print("positive")
```

```error
main.tpn:3:1: error TP0100: expected indented block, found name 'print'
```

## Comprehensions

A *comprehension* builds a list, dict or set from a loop, in one expression:

| Written | Gives |
|---|---|
| `[e for x in xs]` | a `list` of each `e` |
| `{k: v for x in xs}` | a `dict` with each `k` mapped to its `v` |
| `{e for x in xs}` | a `set` of each `e` |
| `(e for x in xs)` | a *generator expression*: an iterator that computes each `e` only when it's asked for |

After the first `for` clause may come more `for` clauses, and `if` clauses, in any order. They nest from left to right, as the same `for` and `if` statements would if each were inside the one before: `[(a, b) for a in xs for b in ys if a != b]` loops over `ys` once for each `a`. An `if` clause skips the values for which it's `False`, and its condition must be a `bool`. A generator expression that's the only argument of a call doesn't need its own parentheses: `sum(x * x for x in xs)`.

```taipan
let squares = [x * x for x in range(6) if x % 2 == 0]
let pairs = [(c, n) for c in "ab" for n in [1, 2]]
let lengths = {word: len(word) for word in ["hi", "taipan"]}
let initials = {name[0] for name in ["ann", "amy", "bob"]}
print(squares)
print(pairs)
print(lengths)
print(initials)
print(sum(x * x for x in range(4)))
```

```output
[0, 4, 16]
[("a", 1), ("a", 2), ("b", 1), ("b", 2)]
{"hi": 2, "taipan": 6}
{"a", "b"}
14
```

The rules:
- **Loop variables** are taken apart as in a `for` loop, so `{k: v * 2 for k, v in d.items()}` works.
- **Scope.** A comprehension's variables are visible only inside it. A variable may have the same name as one outside, which the comprehension leaves unchanged.
- **Types.** With an expected type, from an annotation for example, each element is checked against it, so `let xs: list[float] = [n for n in ints]` turns each `int` into a `float`.
- **Narrowing.** An `if` clause narrows for the clauses after it and for the element, as an `if` statement does, so `[n + 1 for n in maybe if n is not None]` works on a `list[int?]`.
- **When it runs.** A list, dict or set comprehension runs at once. A generator expression computes nothing until values are asked for, except the first `for`'s values, which are found when it's created.
- **Not inside.** `await` can't be inside a comprehension, and assignments and other statements can't either. Write a `for` loop for those.

```taipan
let x = "outside"
let ys = [x * 2 for x in range(3)]
print(x, ys)
let maybe: list[int?] = [1, None, 3]
print([n + 1 for n in maybe if n is not None])
```

```output
outside [0, 2, 4]
[2, 4]
```

```taipan
let ys = [x for x in range(3)]
print(x)
```

```error
main.tpn:2:7: error TP0201: undefined name 'x'
```

```taipan
let ys = [x for x in range(5) if x % 2]
```

```error
main.tpn:1:34: error TP0205: condition must be bool, found int
```
