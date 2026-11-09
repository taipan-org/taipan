---
title: Enums
description: "enum: a type whose values are one of a fixed list of variants, each with its own fields, taken apart with match."
sidebar:
  order: 7
---

An enum is a type whose every value is one of a fixed list of forms, called variants. A variant may carry data in named fields, and different variants may carry different data. Enums are taken apart with [`match`](../match/), which the compiler checks handles every variant. The rules are defined in [§4.4](../../reference/specification/#44-enums-sum-types) of the specification.

## Declaring an enum

`enum Name:` starts an enum, and each indented line names one variant. A variant with data lists its fields in parentheses, each with a name and a type:

```taipan
enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)
    Empty

let shapes = [Circle(1.5), Rect(2, 3), Empty]
print(shapes)
```

```output
[Circle(radius=1.5), Rect(width=2.0, height=3.0), Empty]
```

- A variant without fields, such as `Empty`, is a single value.
- A variant with fields, such as `Circle(radius: float)`, makes a new value each time it's called with its fields' values.
- An enum's body holds only variants: it can't have methods, fields of its own, or defaults for its variants' fields. To compute something from an enum value, write a function that matches on it (below), or use a [class](../classes/).

```taipan
enum Size:
    Small
    Large

    def label(self) -> str:
        return "size"
```

```error
main.tpn:5:5: error TP0100: expected name, found 'def'
```

## Making a value

A variant with fields is called like a function. Its arguments follow the same rules as a function's: by position, in the order the fields are declared, or by name.

```taipan
enum Health:
    Ok
    Degraded(healthy: int, total: int)
    Down

let a = Degraded(1, 4)
let b = Degraded(total=4, healthy=1)
let c = Health.Degraded(1, 4)
let d = Health.Ok
print(a, a == b, b == c, d)
```

```output
Degraded(healthy=1, total=4) True True Ok
```

- A variant can always be written with its enum's name in front, as `Health.Degraded`.
- It can be written alone, as `Degraded`, when no other enum in scope has a variant of that name.
- Every field must be given: a variant with fields can't be used with no arguments, and a variant without fields can't be called.

```taipan
enum Health:
    Ok
    Degraded(healthy: int, total: int)

let a = Degraded(1)
let b = Ok()
let c = Degraded(1, 4, extra=2)
```

```error
main.tpn:5:9: error TP0209: 'Degraded' takes 2 arguments, found 1
main.tpn:6:9: error TP0200: 'Ok' is not a function
main.tpn:7:24: error TP0210: Degraded has no field 'extra'
```

### Two enums with the same variant name

Two enums may each have a variant with the same name. Then the variant must be written with its enum's name, even where the expected type would tell them apart:

```taipan
enum Light:
    Red
    Green

enum Card:
    Red
    Black

let l: Light = Red
```

```error
main.tpn:9:16: error TP0216: 'Red' is ambiguous: it is a variant of Light and Card
  help: qualify it, e.g. 'Light.Red'
```

```taipan
enum Light:
    Red
    Green

enum Card:
    Red
    Black

let l: Light = Light.Red
let c = Card.Red
print(l, c)
```

```output
Red Red
```

A variant's name is also a top-level name of the file, so it can't be the name of a function, constant or class, and a local variable can't take it.

## Reading the fields: `match`

A variant's fields are reached only through `match`, which tests which variant a value is and binds its fields to names. `.radius` on a `Shape` is an error, since not every `Shape` has a radius:

```taipan
enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)
    Empty

def area(s: Shape) -> float:
    match s:
        Circle(r):
            return 3.14159 * r * r
        Rect(w, h):
            return w * h
        Empty:
            return 0.0

def describe(s: Shape) -> str:
    return match s:
        Circle(r) if r > 10: "a big circle"
        Circle(_): "a circle"
        Rect(width=w, height=h) if w == h: "a square"
        Rect(_, _) | Empty: "something else"

for s in [Circle(1), Circle(20), Rect(2, 2), Rect(2, 3), Empty]:
    print(f"{describe(s)}: {area(s):.2f}")
```

```output
a circle: 3.14
a big circle: 1256.64
a square: 4.00
something else: 6.00
something else: 0.00
```

- An arm's pattern `Circle(r)` matches a `Circle` and binds its field to `r`. Fields are matched by position, in declaration order, or by name as `Rect(width=w, height=h)`.
- `_` matches any field without binding it. A variant with fields may also be matched as `Circle` with no parentheses, which matches any `Circle`.
- `match` can be a statement or, after `return`, `let` or `=`, an expression that gives a value.
- The compiler checks that the arms cover every variant. One left out is an error that names it:

```taipan
enum Shape:
    Circle(radius: float)
    Rect(width: float, height: float)
    Empty

def area(s: Shape) -> float:
    match s:
        Circle(r):
            return 3.14159 * r * r
        Rect(w, h):
            return w * h
```

```error
main.tpn:7:5: error TP0204: non-exhaustive match (missing: Empty)
```

```taipan
enum Shape:
    Circle(radius: float)
    Empty

let s = Circle(1.0)
print(s.radius)
```

```error
main.tpn:6:9: error TP0200: 'radius' is a payload field of Shape, which only 'match' can read
  help: write 'match s:' with an arm 'Circle(radius): …'
```

Patterns, guards (`if` after a pattern), `|` and the rest are covered in [match](../match/).

## Equality and display

Two enum values are equal when they're the same variant with equal fields. `print` shows a variant's name, followed by its fields as `name=value`. There's no ordering: `<` on enum values is an error.

```taipan
enum Token:
    Number(value: int)
    Word(text: str)
    End

print(Number(3) == Number(3), Number(3) == Number(4), Word("x") != End)
print([Number(3), Word("hi"), End])
print(f"{Word("hi")}")
```

```output
True False True
[Number(value=3), Word(text="hi"), End]
Word(text="hi")
```

```taipan
enum Size:
    Small
    Large

print(Small < Large)
```

```error
main.tpn:5:7: error TP0200: operator '<' cannot be applied to Size and Size
```

Enum values can't be dict keys or set elements: use a `str` or `int` key, or a [class](../classes/) that defines `__hash__`.

```taipan
enum Size:
    Small
    Large

let prices = {Small: 5, Large: 8}
```

```error
main.tpn:5:15: error TP0200: dict keys must be int, str, bool, tuples of those, or plain class instances, found Size
```

## Fields can't change

An enum value can't be changed once made: its fields are read-only. To get a different value, make a new one.

## Generic enums

An enum may take type parameters in brackets after its name, so that a field's type is chosen where the enum is used:

```taipan
enum Result[T]:
    Success(value: T)
    Failure(message: str)

def parse_age(text: str) -> Result[int]:
    if int(text) is some n:
        return Success(n)
    return Failure(f"not a number: {text}")

for text in ["42", "forty"]:
    match parse_age(text):
        Success(n): print("age", n)
        Failure(why): print("error:", why)
```

```output
age 42
error: not a number: forty
```

- The type arguments are found from the fields' values: `Success(5)` is a `Result[int]`.
- A variant that doesn't use `T`, such as `Failure("x")`, needs an expected type to say what `T` is, from an annotation, a parameter or a return type. Without one it's an error:

```taipan
enum Result[T]:
    Success(value: T)
    Failure(message: str)

let r = Failure("oops")
```

```error
main.tpn:5:9: error TP0206: cannot infer 'T' for 'Failure'
  help: add a type annotation
```

See [Types](../types/#generics) for the rules type parameters follow.

## Variants as function values

A variant with fields can be used as a function without calling it, which makes a value of the variant from its fields:

```taipan
enum Shape:
    Circle(radius: float)
    Empty

let circles = list(map(Circle, [1.0, 2.5]))
print(circles)
print(Circle)
```

```output
[Circle(radius=1.0), Circle(radius=2.5)]
<function Shape.Circle>
```

## Enums in other modules

A module exports its enums and their variants. `import shapes` reaches them as `shapes.Shape.Circle(1.0)` or `shapes.Circle(1.0)`, and `from shapes import Shape` brings in the variants too, so they can be written alone. See [Modules and imports](../modules/).
