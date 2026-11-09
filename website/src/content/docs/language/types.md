---
title: Types
description: "The built-in types, which values fit which types, generics, protocols, type aliases, and object."
sidebar:
  order: 11
---

Every value in Taipan has a type, and the compiler checks, before the program runs, that each value is used only in ways its type allows. This page lists the built-in types, says when a value of one type can be used where another is expected, and covers the ways to write types of your own beyond classes and enums: generics, protocols and type aliases. The rules are defined in [§3.1](../../reference/specification/#31-built-in-types), [§3.3](../../reference/specification/#33-assignability), [§3.6](../../reference/specification/#36-generics-m8), [§3.11](../../reference/specification/#311-protocols-m17), [§3.12](../../reference/specification/#312-type-aliases-m18) and [§3.18](../../reference/specification/#318-types-ii-m40) of the specification.

## The built-in types

| Type | Values | Page |
|---|---|---|
| `int` | whole numbers, 64-bit; going past the range stops the program with an error | [int, float and bool](../../library/numbers/) |
| `float` | numbers with a fractional part, 64-bit floating point | [int, float and bool](../../library/numbers/) |
| `bool` | `True` and `False` | [int, float and bool](../../library/numbers/#bool) |
| `str` | text, which can't be changed | [str](../../library/str/) |
| `list[T]` | an ordered sequence of `T`s that can grow and shrink | [list](../../library/list/) |
| `dict[K, V]` | a map from keys of type `K` to values of type `V`, in the order the keys were added | [dict](../../library/dict/) |
| `set[T]` | a collection of distinct `T`s | [set](../../library/set/) |
| `tuple[A, B]` | a fixed number of values, each with its own type; can't be changed | [tuple](../../library/tuple/) |
| `tuple[T, ...]` | any number of `T`s, as a tuple | [tuple](../../library/tuple/) |
| `T?` | a `T` or `None` | [Optionals and narrowing](../optionals/) |
| `Callable[[A, B], R]` | a function taking an `A` and a `B` and returning an `R` | [Functions](../functions/#functions-as-values-and-callable) |
| `Iterator[T]`, `Iterable[T]`, `Generator[Y, S, R]` | values produced one at a time | [Iterators and generators](../generators/) |
| `AsyncIterator[T]`, `AsyncIterable[T]`, `AsyncGenerator[Y, S]` | the same, waited for with `async for` | [async and await](../async/) |
| `TextIO` | an open text file, or standard input or output | [Files](../../library/files/) |
| `Exception`, `ValueError`, … | exceptions | [Exceptions](../../library/exceptions/) |
| `object` | a value of any type (below) | |
| `py` | a value from Python | [Calling Python](../../guides/python/) |

A class, an enum, a protocol or a type alias you declare is a type too. `None` is the only value of its own type, which fits every optional type `T?`. A function declared without `->` returns nothing, and its call can't be used as a value.

### Types are checked, not guessed

- Every function parameter has a written type. A function's result type is written, or it returns nothing.
- A variable without a written type takes the type of its first value: `let n = 3` is an `int`. Its type never changes after that.
- An empty `[]` or `{}`, or `None`, needs a written type to say what it will hold: `let names: list[str] = []`.
- A list's elements must share a type, except that `int`s and `float`s together make a `list[float]`, and instances of classes with a common base class make a list of that base.

```taipan
let empty = []
```

```error
main.tpn:1:13: error TP0206: cannot infer the type of an empty list
  help: add a type annotation
```

## Which values fit which types

A value of type `S` can be used where a `T` is expected, as a variable's value, an argument or a result, when one of these holds:

| Rule | Example |
|---|---|
| `S` and `T` are the same type | `str` for `str` |
| `S` is `int` and `T` is `float` | `3` where a `float` is expected becomes `3.0` |
| `T` is optional, `U?`, and `S` fits `U` | `3` for an `int?`; `None` for any optional |
| `S` is a class derived from the class `T` | a `Dog` for an `Animal`, and a `Dog?` for an `Animal?` |
| `T` is a protocol that `S` satisfies | a `Circle` for a `Drawable` (below) |
| both are function types whose parameters and result fit (below) | a `Callable[[Animal], Dog]` for a `Callable[[Dog], Animal]` |

Nothing else converts on its own. To turn one type into another, call `int(x)`, `float(x)`, `str(x)` and the other conversion functions, or use `as` between related classes ([Classes](../classes/#checking-and-converting-the-class)).

```taipan
let count: int = 3
let ratio: float = count
let label: str = count
```

```error
main.tpn:3:18: error TP0200: expected str, found int
```

```taipan
let count = 3
let label: str = str(count)
let parsed = int("42") ?? 0
print(label, parsed + 1)
```

```output
3 43
```

### Function types

For a function to fit where a function type is expected, each parameter type it's given must fit the function's own parameter, and its result must fit the expected result. So a function that accepts any `Animal` can be used where one that accepts a `Dog` is expected, and one that returns a `Dog` where one that returns an `Animal` is expected.

### Lists, dicts and other generic types don't vary

A `list[Dog]` can't be used as a `list[Animal]`, even though a `Dog` is an `Animal`. Code holding the list as a `list[Animal]` could add a `Cat` to it, and then the code holding it as a `list[Dog]` would find a `Cat` in a list of dogs. The same holds for `dict`, `set`, tuples, and your own generic classes. A list written out where a `list[Animal]` is expected is checked against that type, so `let pets: list[Animal] = [Dog("rex")]` is valid.

```taipan
class Animal:
    pass

class Dog(Animal):
    pass

def count(pets: list[Animal]) -> int:
    return len(pets)

let dogs: list[Dog] = [Dog(), Dog()]
print(count(dogs))
```

```error
main.tpn:11:13: error TP0200: expected list[Animal], found list[Dog]
```

```taipan
class Animal:
    pass

class Dog(Animal):
    pass

def count(pets: list[Animal]) -> int:
    return len(pets)

let dogs: list[Animal] = [Dog(), Dog()]
print(count(dogs))
```

```output
2
```

## Generics

A generic function or class works for many types. Its type parameters, written in brackets after its name, stand for types that are chosen where it's used:

```taipan
def first[T](xs: list[T]) -> T?:
    if len(xs) == 0:
        return None
    return xs[0]

class Box[T]:
    item: T

    def __init__(self, item: T):
        self.item = item

    def get(self) -> T:
        return self.item

enum Tree[T]:
    Leaf(value: T)
    Pair(left: T, right: T)

print(first([3, 1, 2]), first(["a"]), first([1.5]))
let b = Box("hello")
print(b.get().upper())
let t: Tree[int] = Pair(1, 2)
print(t)
```

```output
3 a 1.5
HELLO
Pair(left=1, right=2)
```

- Functions, classes and enums may take type parameters. A class's methods use the class's type parameters and take none of their own. A nested `def` can't have type parameters.
- **Type arguments are always worked out by the compiler**, from the arguments of a call and from the type the result is expected to have. There's no syntax for giving them at a call. `Box("hello")` is a `Box[str]`.
- In a type, a generic class or enum takes all its type arguments: `Box[int]`, `Tree[str]`. A bare `Box` is an error.

### What a type parameter allows

Inside a generic function, the compiler knows nothing about a `T`, so it allows only what works for every type: assigning and passing it, returning it, storing it in lists and fields, comparing two `T`s with `==` and `!=`, `in` on a `list[T]`, and printing it. Anything else, such as arithmetic, `<`, or calling a method, is an error, since some type argument wouldn't support it:

```taipan
def largest[T](xs: list[T]) -> T:
    var best = xs[0]
    for x in xs:
        if x > best:
            best = x
    return best
```

```error
main.tpn:4:12: error TP0200: operator '>' cannot be applied to T and T
```

There are two ways to allow more: a bound, and a list of allowed types.

### Bounds

A bound, written `[T: Bound]`, says that every type argument must have what `Bound` describes. In the body, a `T` then has the bound's methods and operators. The bound is a protocol (below) or a class.

`Comparable` is a built-in protocol for types with `<`: `int`, `float`, `str`, tuples of them, and classes that define `__lt__`. A `Comparable` type parameter allows `<` and `>`, and lists of it can be sorted:

```taipan
def largest[T: Comparable](xs: list[T]) -> T:
    var best = xs[0]
    for x in xs:
        if x > best:
            best = x
    return best

print(largest([3, 9, 4]), largest(["pear", "apple"]))
```

```output
9 pear
```

A call whose type argument doesn't satisfy the bound is an error, at the call:

```taipan
def largest[T: Comparable](xs: list[T]) -> T:
    return sorted(xs)[-1]

print(largest([True, False]))
```

```error
main.tpn:4:7: error TP0200: 'bool' does not satisfy 'Comparable': it has no method '__lt__'
```

A class bound, `[A: Animal]`, takes `Animal` or any class derived from it, and in the body an `A` has `Animal`'s fields and methods. The function's result keeps the exact class it was given, where a parameter of type `Animal` would lose it:

```taipan
class Animal:
    volume: int

    def __init__(self, volume: int):
        self.volume = volume

class Dog(Animal):
    def fetch(self) -> str:
        return "fetched"

def loudest[A: Animal](xs: list[A]) -> A:
    return max(xs, key=lambda a: a.volume)

print(loudest([Dog(3), Dog(7)]).fetch())
```

```output
fetched
```

Bounds go only on a function's type parameters, not a class's or an enum's.

### Allowed types

`[N: (int, float)]` lists the types `N` may be. The body is checked once for each of them, so it may use whatever all of them support, such as `+` for numbers:

```taipan
def total[N: (int, float)](xs: list[N]) -> N:
    var sum = xs[0]
    for x in xs[1:]:
        sum = sum + x
    return sum

print(total([1, 2, 3]), total([0.5, 0.25]))
```

```output
6 0.75
```

The list needs at least two types, none of them optional. Like bounds, it's only for a function's type parameters.

## Protocols

A protocol describes what a type must have, its methods and fields, without the type having to derive from anything. A type satisfies a protocol simply by having everything the protocol lists, with matching parameter and result types. This is called structural typing: it goes by what a type has, not by what it's declared to be.

A protocol is written as a class whose base is `Protocol`, with `...` as each method's body:

```taipan
class Drawable(Protocol):
    name: str
    def draw(self) -> str: ...

class Circle:
    name: str
    r: float

    def __init__(self, r: float):
        self.name = "circle"
        self.r = r

    def draw(self) -> str:
        return f"({self.r})"

class Label:
    name: str

    def __init__(self, name: str):
        self.name = name

    def draw(self) -> str:
        return f"[{self.name}]"

let items: list[Drawable] = [Circle(1.5), Label("hi")]
for d in items:
    print(d.name, d.draw())
```

```output
circle (1.5)
hi [hi]
```

- `Circle` and `Label` don't mention `Drawable`, and share no base class, but both satisfy it.
- A protocol can be a type, as `list[Drawable]` is here, or a bound, as `[T: Drawable]`. Calling a method through a protocol value runs the method of the object's own class.
- A protocol can't be constructed, and isn't a class to test with `isinstance`. Test for one of the classes that satisfy it instead.
- A protocol's methods take `self` first, annotate their other parameters, have no defaults, and have `...` as their body. Its fields have no defaults. Its only special method may be `__lt__`.
- A type satisfies a field only with a field of exactly that type, not a property.

The compiler says what's missing when a type doesn't satisfy a protocol:

```taipan
class Drawable(Protocol):
    def draw(self) -> str: ...

class Point:
    x: int = 0

let d: Drawable = Point()
```

```error
main.tpn:7:19: error TP0200: 'Point' does not satisfy 'Drawable': it has no method 'draw'
```

### Declaring a protocol on a class

A class may list protocols among its bases, `class Circle(Drawable):`. That doesn't change what the class satisfies, but the compiler then checks the class against the protocol where it's declared, instead of where it's first used, and it documents the intent. Protocols listed this way don't count toward the one base class a class may have.

```taipan
class Drawable(Protocol):
    def draw(self) -> str: ...

class Point(Drawable):
    x: int = 0

    def draw(self, scale: int) -> str:
        return "."
```

```error
main.tpn:4:13: error TP0200: 'Point' does not satisfy 'Drawable': 'draw' must be 'def draw(self) -> str'
```

### `Self` in a protocol

In a protocol method, `Self` stands for the type that satisfies the protocol: `def scaled(self, k: float) -> Self: ...` is satisfied by a `Square` whose `scaled` returns a `Square`.

A protocol that uses `Self` anywhere other than as a method's whole result, such as `Comparable`, whose `__lt__` takes `other: Self`, can only be a bound, not a type. A value of the protocol's type couldn't check that another value has the same class:

```taipan
def show(x: Comparable):
    print(x)
```

```error
main.tpn:1:13: error TP0229: 'Comparable' uses 'Self' in '__lt__', so it can only be a type parameter bound
  help: use a bounded type parameter, e.g. 'def f[T: Comparable](x: T)'
```

### Generic protocols

A protocol may take type parameters: `class Source[T](Protocol):` with `def take(self) -> T?: ...`. Then `Source[int]` is satisfied as `Source` is with `T` replaced by `int`.

## Type aliases: `type`

`type Name = T` gives a type a shorter or clearer name. The alias is exactly the type it names, not a new type: a value of one is a value of the other.

```taipan
type Point = tuple[float, float]
type Pair[T] = tuple[T, T]
type Handler = Callable[[str], str]

def midpoint(a: Point, b: Point) -> Point:
    return (a[0] + b[0]) / 2, (a[1] + b[1]) / 2

def swap[T](p: Pair[T]) -> Pair[T]:
    return p[1], p[0]

let shout: Handler = lambda s: s.upper() + "!"
print(midpoint((0.0, 0.0), (4.0, 2.0)), swap((1, 2)), shout("hi"))
```

```output
(2.0, 1.0) (2, 1) HI!
```

- An alias may be declared at the top level, inside a block, or inside a class body, where only the class sees it. A top-level alias can be used before the line that declares it.
- A generic alias, such as `Pair[T]`, takes all its type arguments where it's used: `Pair[int]`.
- An alias is only a type, not a value. It can't be called or constructed, or used as a base class: use the class it names.
- An alias can't refer to itself, directly or through other aliases, since expanding it would never end. A recursive structure needs a class: `class Tree:` with `children: list[Tree]`.
- Error messages show the alias's name: `expected Handler, found int`.

```taipan
type Tree = list[Tree]
```

```error
main.tpn:1:6: error TP0230: type alias 'Tree' refers to itself
  help: use a class for a recursive type
```

```taipan
type Handler = Callable[[str], str]

let h: Handler = 5
```

```error
main.tpn:3:18: error TP0200: expected Handler, found int
```

## `object`

`object` holds a value of any type. Any value can be given where an `object` is expected, but an `object` allows almost nothing: it can be printed, compared with `None` using `is None` and `is not None`, tested with `isinstance`, and converted back with `as`:

- `x as int` gives the `int` it holds, and raises `TypeError` if it holds something else.
- `x as int?` gives `None` instead of raising.
- `isinstance(x, int)` tests it, and inside an `if`, `x` can be used as an `int`.

```taipan
def describe(x: object) -> str:
    if isinstance(x, int):
        return f"an int, doubled: {x * 2}"
    if x as str? is some s:
        return f"the text {s.upper()}"
    return f"something else: {x}"

print(describe(41))
print(describe("hi"))
print(describe([1, 2]))
```

```output
an int, doubled: 82
the text HI
something else: [1, 2]
```

`object` is mostly met as the type of `StopIteration.value` and of values from .NET libraries. In your own code, a protocol, a generic type parameter or an enum usually says more.

## Related pages

- `T?` and how checks narrow a type: [Optionals and narrowing](../optionals/).
- `Self` in a class's methods: [Classes](../classes/#self).
- Classes and enums, the types you declare most: [Classes](../classes/), [Enums](../enums/).
