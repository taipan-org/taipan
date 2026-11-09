---
title: match
description: "match statements and expressions, every kind of pattern, guards, exhaustiveness and unreachable arms."
sidebar:
  order: 4
---

`match` compares one value, the *subject*, against a list of *patterns*, and runs the first arm whose pattern fits it. A pattern can test what kind of value it is, compare it with a literal, and take it apart, naming the pieces. The compiler checks that the arms cover every value the subject could have.

```taipan
enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)
    Dot

def area(s: Shape) -> float:
    match s:
        Circle(r):
            return 3.14159 * r * r
        Rect(w, h):
            return w * h
        Dot:
            return 0.0

print(area(Circle(1.0)), area(Rect(2.0, 3.0)), area(Dot))
```

```output
3.14159 6.0 0.0
```

`match` is defined in [§5.1](../../reference/specification/#51-match), and `match` expressions in [§5.1.1](../../reference/specification/#511-match-expressions-m19).

## Arms

After `match subject:` comes an indented list of arms. Each arm is a pattern, an optional [guard](#guards), a colon, and then either one statement on the same line or an indented block:

```taipan
let code = 404
match code:
    200: print("ok")
    404:
        print("not found")
        print("check the address")
    _: print("something else")
```

```output
not found
check the address
```

The subject is computed once. The arms are then tried from the top, and only the first one that matches runs. Each arm's pattern is written on its own, without a keyword in front of it.

`break` and `continue` in an arm apply to the loop around the `match`, and `return` returns from the function, as they would anywhere else.

## What can be matched

The subject may be:
- an enum, an `int`, a `float`, a `str` or a `bool`
- an object of a class
- a tuple, a list or a dict
- an optional (`T?`) of any of these

Anything else, such as a function or a set, is an error:

```taipan
let s = {1, 2}
match s:
    _: print("a set")
```

```error
main.tpn:2:7: error TP0200: cannot match on set[int]
```

## Patterns

| Pattern | Matches | Binds |
|---|---|---|
| `_` | anything | nothing |
| `name` | anything | `name` to the value |
| `0`, `-1`, `2.5`, `"api"`, `True`, `False` | a value equal to the literal | nothing |
| `None` | `None`, at an optional position | nothing |
| `Variant`, `Variant(p, …)`, `Enum.Variant` | that variant of an enum, with each field matching its pattern | what the field patterns bind |
| `Class()`, `Class(field=p, …)`, `Class(p, …)` | an object of that class or a class derived from it | what the field patterns bind |
| `(p1, p2, …)` | a tuple whose elements match, one pattern per element | what each pattern binds |
| `[p1, p2, …]`, `[p, *rest]` | a list of that length, or at least that length with `*` | what each pattern binds, and `rest` to the other elements |
| `{"key": p, …}`, `{"key": p, **rest}` | a dict that has each key, with its value matching | what each pattern binds, and `rest` to the other entries |
| `p1 \| p2 \| …` | a value any alternative matches | the names every alternative binds |
| `p as name` | what `p` matches | `name` to the whole value, and what `p` binds |
| `(p)` | what `p` matches; the parentheses only group | what `p` binds |

Patterns nest: wherever a pattern is expected inside another one, any kind may appear, as in `Rect(0.0, h)` or `[(x, 0), *_]`.

### `_` and names

`_` matches any value and ignores it. It may appear any number of times in one pattern.

A plain name matches any value and *binds* it: the name holds the value inside the arm, as a `let` would. Bound names follow the usual rules for names (see [Scope](../variables/#scope)), so a name already visible outside the `match` can't be bound again, and one name can't be bound twice in one pattern.

```taipan
let n = 42
match n:
    0: print("zero")
    other: print(f"got {other}")
```

```output
got 42
```

A name is a binding unless it names something: a variant of the enum expected at that position, or a class. A bare name that starts with a capital letter, where an enum is expected, and isn't one of its variants, is an error rather than a binding, so that a misspelled variant isn't silently treated as a new name:

```taipan
enum Health:
    Ok
    Down

let h = Health.Ok
match h:
    Ok: print("up")
    Dwn: print("down")
```

```error
main.tpn:8:5: error TP0210: Health has no variant 'Dwn'
```

### Literals

An integer, float or string literal, `True` or `False` matches a value equal to it. A literal may have a leading `-`. Integer literals also match a `float` subject, so `0` matches `0.0`, and `0.0` matches `-0.0`. A literal must have the subject's type, so `"1"` can't match an `int`.

```taipan
def describe(x: float) -> str:
    return match x:
        0: "zero"
        1.5: "one and a half"
        _: "something else"

print(describe(-0.0), describe(1.5), describe(2.0))
```

```output
zero one and a half something else
```

### Enum variants

A variant without fields is matched by its name, with or without `()`. A variant with fields is matched by its name followed by one pattern per field, by position, or by its name alone, which matches any field values. The name may be qualified with the enum's name, `Health.Down`.

```taipan
enum Health:
    Ok
    Degraded(healthy: int, total: int)
    Down

def describe(h: Health) -> str:
    return match h:
        Ok: "up"
        Degraded(0, _): "failing"
        Degraded(healthy, total): f"{healthy} of {total} healthy"
        Health.Down: "down"

print(describe(Ok), describe(Degraded(0, 3)), describe(Degraded(2, 3)), describe(Down))
```

```output
up failing 2 of 3 healthy down
```

A variant pattern must have one pattern for each field:

```taipan
enum Health:
    Ok
    Degraded(healthy: int, total: int)

let h = Degraded(1, 2)
match h:
    Ok: print("up")
    Degraded(n): print(n)
```

```error
main.tpn:8:5: error TP0209: 'Degraded' has 2 fields, found 1 binding
```

### Classes

`Class(…)` matches an object of that class, or of a class derived from it, as `isinstance` tests. Inside the parentheses, `field=pattern` matches a field by name, in any order, including inherited fields. For a dataclass, patterns may also be given by position, in the order of its fields. `Class()` with nothing inside matches any object of the class. In the arm, a subject that's a name that can't change (a `let` or a parameter) is treated as that class, so its own fields and methods can be used. Classes are described on [Classes](../classes/).

```taipan
@dataclass
class Point:
    x: int
    y: int

def where(p: Point) -> str:
    return match p:
        Point(0, 0): "at the origin"
        Point(0, y): f"on the y axis at {y}"
        Point(x=x, y=0): f"on the x axis at {x}"
        _: "elsewhere"

print(where(Point(0, 0)), where(Point(0, 5)), where(Point(3, 0)), where(Point(1, 1)))
```

```output
at the origin on the y axis at 5 on the x axis at 3 elsewhere
```

A class pattern always has parentheses, unlike a variant:

```taipan
class Cat:
    pass

let c = Cat()
match c:
    Cat: print("a cat")
```

```error
main.tpn:6:5: error TP0200: 'Cat' is a class; write 'Cat()'
```

Patterns by position work only on a dataclass, a field may be named only once, and the class must be the subject's class or one derived from it.

### Tuples

`(p1, p2, …)` matches a tuple with one pattern per element. One pattern may be `*name`, which binds the elements it stands for as a list, or `*_`, which ignores them.

```taipan
def check(t: tuple[int, bool]) -> str:
    return match t:
        (0, _): "zero"
        (n, True): f"{n}, yes"
        (n, False): f"{n}, no"

print(check((0, True)), check((4, True)), check((5, False)))
```

```output
zero 4, yes 5, no
```

A tuple pattern must have as many patterns as the tuple has elements:

```taipan
let t = (1, 2, 3)
match t:
    (a, b): print(a, b)
```

```error
main.tpn:3:5: error TP0209: tuple[int, int, int] has 3 elements, found 2 patterns
```

`(p)` with one pattern is a group, not a tuple. A one-element tuple pattern, `(p,)`, is an error, since tuples have at least two elements.

### Lists

`[p1, p2, …]` matches a list with exactly that many elements, each matching its pattern. With a `*name` among them, it matches a list with at least as many elements as the other patterns, and `name` holds the elements in between as a new list. `*_` ignores them. Only one `*` may appear in a list pattern.

```taipan
def show(xs: list[int]) -> str:
    return match xs:
        []: "empty"
        [x]: f"just {x}"
        [first, *_, last]: f"{first} to {last}"

print(show([]), show([7]), show([1, 2, 3, 4]))
```

```output
empty just 7 1 to 4
```

### Dicts

`{key: pattern, …}` matches a dict that has every listed key, with each key's value matching its pattern. Other keys are ignored, so `{}` matches every dict. Keys are literals of the dict's key type, which must be `int`, `str` or `bool`, and a key may be listed once. A final `**name` binds a new dict of the entries the pattern didn't list.

```taipan
def server(d: dict[str, int]) -> str:
    return match d:
        {"port": 80}: "plain web"
        {"port": p, **others}: f"port {p}, and {len(others)} other settings"
        {}: "no port"

print(server({"port": 80}), server({"port": 8080, "debug": 1}), server({}))
```

```output
plain web port 8080, and 1 other settings no port
```

`**` must come last, and `**_` is an error: leave it out instead, since other keys are ignored anyway.

### `None` and optionals

When the subject, or a part of it, has an optional type `T?`, the pattern `None` matches `None`. Every other pattern except `_` and a plain name matches only a value that isn't `None`, and checks it as a `T`. A plain name at an optional position binds a `T?`, which may still be `None`. The exception is a name that's the whole pattern, after earlier arms without guards have matched `None`: it binds a `T`, as `n` does here:

```taipan
def describe(x: int?) -> str:
    return match x:
        None: "nothing"
        0: "zero"
        n: f"one more is {n + 1}"

print(describe(None), describe(0), describe(41))
```

```output
nothing zero one more is 42
```

`None` anywhere that isn't optional is an error:

```taipan
let n = 3
match n:
    None: print("none")
    _: print("some")
```

```error
main.tpn:3:5: error TP0200: int can't be None
```

### Or-patterns

`p1 | p2 | …` matches when any of its alternatives does, tried from the left. Every alternative must bind the same names, with the same types (classes may differ, and the name then has their nearest common base class).

```taipan
def answer(s: str) -> str:
    return match s:
        "y" | "yes" | "sure": "agreed"
        _: "not agreed"

print(answer("yes"), answer("no"))
```

```output
agreed not agreed
```

```taipan
let t = (1, 2)
match t:
    (a, 0) | (0, b): print("on an axis")
    _: print("elsewhere")
```

```error
main.tpn:3:14: error TP0200: alternatives bind different names: 'a' is not bound in every one
```

An alternative that can never match anything the alternatives before it don't is warning `TP0300`, `unreachable alternative`.

### `as`

`pattern as name` matches what `pattern` matches, and binds `name` to the whole matched value. `as` applies to everything before it, so `A | B as x` means `(A | B) as x`. Parentheses group a pattern, so `Point((0 | 1) as x, y)` names just one field.

```taipan
enum Token:
    Number(value: int)
    Plus
    Minus

def show(t: Token) -> str:
    return match t:
        (Plus | Minus) as op: f"operator {op}"
        Number(n) as whole: f"{whole} holds {n}"

print(show(Plus), show(Number(7)))
```

```output
operator Plus Number(value=7) holds 7
```

## Guards

An arm may follow its pattern with `if condition`. The arm runs only when the pattern matches *and* the condition is `True`. The condition can use the names the pattern binds, and must be a `bool`. When it's `False`, the next arm is tried.

```taipan
def classify(value: int) -> str:
    return match value:
        0: "zero"
        n if n < 0: "negative"
        n if n % 2 == 0: "even"
        _: "odd"

print(classify(0), classify(-3), classify(4), classify(7))
```

```output
zero negative even odd
```

## Exhaustiveness

A `match` must cover every value its subject could have. If some value would reach the end without matching any arm, that's error `TP0204`, which lists patterns for the values that aren't covered:

```taipan
enum Health:
    Ok
    Degraded(healthy: int, total: int)
    Down

def describe(h: Health) -> str:
    match h:
        Ok:
            return "up"
        Degraded(0, _):
            return "failing"
```

```error
main.tpn:7:5: error TP0204: non-exhaustive match (missing: Degraded(_, _), Down)
```

What covers a type:

| Type | Covered by |
|---|---|
| an enum | every variant, with patterns covering each one's fields |
| `bool` | `True` and `False` |
| a tuple, or an object of the subject's own class | a pattern whose element or field patterns cover each element or field |
| a list | patterns covering every length, such as `[]` and `[first, *rest]` |
| a dict | a pattern with no keys, `{}` or `{**rest}` |
| an optional `T?` | `None`, together with patterns that cover `T` |
| `int`, `float`, `str`, or a class derived from the subject's class | only `_` or a plain name |

So when you add a variant to an enum, the compiler finds every `match` that needs a new arm. `_` or a name, as the last arm, covers everything that's left.

```taipan
let ok = True
let s = match ok:
    True: "yes"
print(s)
```

```error
main.tpn:2:9: error TP0204: non-exhaustive match (missing: False)
```

```taipan
let xs = [1, 2]
match xs:
    []: print("empty")
    [a]: print(a)
```

```error
main.tpn:2:1: error TP0204: non-exhaustive match (missing: [_, _, *_])
```

**A guarded arm covers nothing**, since its condition may be `False`, so a value covered only by guarded arms still needs another arm:

```taipan
let n = 5
match n:
    x if x > 0: print("positive")
    0: print("zero")
```

```error
main.tpn:2:1: error TP0204: non-exhaustive match (missing: _)
```

## Unreachable arms

An arm that can't match anything the arms before it don't match, such as one after `_`, or a repeated one, is warning `TP0300`. The program still compiles and runs, and the arm is checked for errors, but it never runs:

```taipan
let n = 1
match n:
    _: print("any")
    1: print("one")
```

```output
any
```

```error
main.tpn:4:5: warning TP0300: unreachable match arm
```

## `match` expressions

A `match` can also give a value. Each arm is then a pattern, a colon and an expression, and the value of the `match` is the value of the arm that matched. It may appear anywhere an expression may: after `=`, after `return`, as an operand or as an argument. Its `:` must end its line, and its arms are indented under it.

```taipan
let n = 7
let label = match n % 3:
    0: "fizz"
    _: "no fizz"
let total = 10 + match n:
    7: 1
    _: 0
print(label, total)
print(match n > 5:
    True: "big"
    False: "small"
, "number")
```

```output
no fizz 11
big number
```

Inside parentheses, as in the `print` above, the arms end at the first line indented no further than the line with `match`, and the rest of the expression continues there.

The rules:
- **Patterns and checks** are those of the statement: the same patterns, guards, exhaustiveness and unreachable arms.
- **The type.** With an expected type, from an annotation, a function's return type or an assignment's target, each arm is checked against it. Otherwise the arms must agree: the same type, `int` and `float` together giving `float`, classes giving their nearest common base, and `None` with a `T` giving a `T?`.
- **Block arms.** An arm may be an indented block. Its statements run in order, and the last one gives the arm's value, so it must be an expression (or a `raise`). Names declared in the block are visible only in it.
- **Raising arms.** An arm may be `raise …`, which gives no value. At least one arm must give a value.
- **No leaving.** `return`, and `break` or `continue` for a loop outside the `match`, can't be used in an arm of a `match` expression, since it's in the middle of computing a value. Use a `match` statement for that.
- **At the start of a line**, `match` is always a statement. To use a `match` expression's value there, assign it.

```taipan
let n = 7
let x: float = match n:
    7: 1
    _: 2.5
let size = match n:
    0: None
    _: n * 10
let cost = match n % 2:
    0: 1
    _:
        let half = n // 2
        half * 3
print(x, size, cost)
```

```output
1.0 70 9
```

```taipan
def parse(s: str) -> int:
    return match s:
        "one": 1
        "two": 2
        _: raise ValueError(f"unknown number '{s}'")

print(parse("two"))
print(parse("six"))
```

```output
2
```

```error
Traceback (most recent call last):
  File "main.tpn", line 8, in <module>
    print(parse("six"))
  File "main.tpn", line 2, in parse
    return match s:
ValueError: unknown number 'six'
```

```taipan
let n = 1
let x = match n:
    0: 1
    _: "one"
```

```error
main.tpn:4:8: error TP0200: expected int, found str
```

```taipan
let n = 1
let x = match n:
    0: 1
    _:
        let y = 2
```

```error
main.tpn:5:9: error TP0200: this arm gives no value
  help: end it with an expression
```

```taipan
def f(n: int) -> int:
    let x = match n:
        0:
            return 1
        _: n
    return x
```

```error
main.tpn:4:13: error TP0215: 'return' can't leave a 'match' expression
  help: use a 'match' statement
```

```taipan
let n = 1
let x = match n:
    _: raise ValueError("no")
```

```error
main.tpn:2:9: error TP0200: every arm of this 'match' raises
  help: use a 'match' statement
```
