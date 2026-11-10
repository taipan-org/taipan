---
title: Classes
description: "class, fields, __init__, methods, dataclasses, inheritance, super(), properties, static and class methods, abstract classes, and special methods."
sidebar:
  order: 6
---

A class defines a new type of object: the data each object holds, called its fields, and the functions that work on it, called its methods. An object made from a class is an instance of it. The rules are defined in [§4.3](../../reference/specification/#43-classes-m15), [§4.6](../../reference/specification/#46-classes-lists-and-dicts-checking-rules), [§4.8](../../reference/specification/#48-special-methods-and-class-members-m28) and [§4.9](../../reference/specification/#49-classes-iii-m39) of the specification.

## Declaring a class

`class Name:` starts a class, and its indented body declares fields and methods.

```taipan
class Account:
    owner: str
    balance: float = 0.0

    def __init__(self, owner: str):
        self.owner = owner

    def deposit(self, amount: float):
        self.balance += amount

    def describe(self) -> str:
        return f"{self.owner} has {self.balance}"

let acct = Account("Ada")
acct.deposit(25)
acct.deposit(17.5)
print(acct.describe())
print(acct.balance)
```

```output
Ada has 42.5
42.5
```

- A **field** is declared in the class body with a name and a type, and optionally a default: `balance: float = 0.0`. Every instance has its own copy of each field.
- **`__init__`** is the constructor: the method that runs when an instance is made. Calling the class's name, `Account("Ada")`, makes a new instance and passes the arguments to `__init__`.
- A **method** is a function declared in the class body. Its first parameter is `self`, the instance it was called on, written without a type. It's called as `acct.deposit(25)`, and `acct` becomes `self`.
- Inside a method, fields are reached as `self.balance` and other methods as `self.describe()`. A bare `describe()` would mean a top-level function.
- Methods take parameters of every kind a function does, with defaults and keyword arguments: see [Functions](../functions/).
- Classes are declared at the top level of a file only, not inside a function. A class can be used before the line that declares it.

### Fields must be declared

Every field must be declared in the class body. Assigning one that isn't is an error, which catches a misspelled field name:

```taipan
class Point:
    x: float

    def __init__(self, x: float):
        self.x = x
        self.y = 0.0
```

```error
main.tpn:6:14: error TP0210: Point has no field 'y'
```

A field's default is evaluated again for each new instance, so a field with a default of `[]` gives each instance its own list:

```taipan
class Basket:
    items: list[str] = []

let a = Basket()
let b = Basket()
a.items.append("apple")
print(a.items, b.items)
```

```output
["apple"] []
```

(Python evaluates a class-level default once and shares it between instances; see [differences from Python](../../reference/python-differences/).)

A field can be changed through any variable holding the object, even a `let`: `let` fixes which object the variable holds, not what's inside the object.

### Every field is set before the object is used

The compiler checks that `__init__` sets every field that has no default, on every path through it. An object can never be seen with a field unset:

```taipan
class User:
    name: str
    email: str

    def __init__(self, name: str, email: str):
        self.name = name
        if email != "":
            self.email = email
```

```error
main.tpn:5:9: error TP0226: '__init__' does not set 'email'
  help: assign 'self.email' in '__init__', or give the field a default
```

Until every field is set, `self` may only be used to set fields, or to read a field already set. Calling a method or passing `self` somewhere is an error, since that code could read a field that isn't set yet:

```taipan
class User:
    name: str
    email: str

    def __init__(self, name: str, email: str):
        self.name = name
        print(self.describe())
        self.email = email

    def describe(self) -> str:
        return f"{self.name} <{self.email}>"
```

```error
main.tpn:7:15: error TP0226: 'self' is used before 'email' is set
```

A class without an `__init__` and without a base class takes no arguments, and needs a default for each of its fields.

### Display and equality of a plain class

A class like the ones above, without `@dataclass` (below), is a plain class:

- `print` shows it as `<Account object>`, unless the class defines `__str__` or `__repr__` (see [Special methods](#special-methods)).
- `==` compares identity: an instance is equal only to itself, unless the class defines `__eq__`.
- It can be a dict key or a set element, compared by identity.

```taipan
class Tag:
    name: str

    def __init__(self, name: str):
        self.name = name

let t = Tag("x")
print(t)
print(t == t, t == Tag("x"))
let counts = {t: 1}
print(counts[t])
```

```output
<Tag object>
True False
1
```

## Dataclasses: `@dataclass`

`@dataclass` on the line before a class makes a class whose constructor is written for you: it takes the fields, in the order they're declared, with their defaults. A dataclass compares field by field and displays its fields.

```taipan
@dataclass
class Point:
    x: float
    y: float = 0.0

    def length(self) -> float:
        return (self.x ** 2 + self.y ** 2) ** 0.5

let p = Point(3, 4)
print(p, p.length())
print(Point(1.5))
print(Point(y=2, x=1) == Point(1, 2))
```

```output
Point(x=3.0, y=4.0) 5.0
Point(x=1.5, y=0.0)
True
```

- A dataclass can't define `__init__`. To check or adjust the fields after they're set, define `__post_init__(self) -> None`, which runs at the end of the constructor.
- A field without a default can't follow one with a default, since the constructor's parameters follow the same rule as a function's.
- An argument for a field that doesn't exist is an error, and so is a missing one:

```taipan
@dataclass
class Point:
    x: float
    y: float = 0.0

print(Point(y=1.0))
print(Point(1.0, z=2.0))
```

```error
main.tpn:6:7: error TP0209: missing field 'x' in 'Point'
main.tpn:7:18: error TP0210: Point has no field 'z'
```

```taipan
@dataclass
class Range:
    low: int
    high: int

    def __post_init__(self) -> None:
        if self.low > self.high:
            raise ValueError(f"empty range {self.low}..{self.high}")

print(Range(1, 5))
print(Range(5, 1))
```

```output
Range(low=1, high=5)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 11, in <module>
    print(Range(5, 1))
  File "main.tpn", line 8, in __post_init__
    raise ValueError(f"empty range {self.low}..{self.high}")
ValueError: empty range 5..1
```

### `frozen=True` and `order=True`

`@dataclass` takes two options, each `True` or `False`:

| Option | Effect |
|---|---|
| `frozen=True` | fields can't be assigned once the object exists, checked at compile time; the class can be a dict key or set element, hashed by its fields |
| `order=True` | `<`, `<=`, `>` and `>=` compare the fields in order, as a tuple does, so instances can be sorted |

A plain `@dataclass` (not frozen) can't be a dict key, since its fields could change while it's in the dict.

```taipan
@dataclass(frozen=True, order=True)
class Version:
    major: int
    minor: int = 0

let versions = [Version(2, 1), Version(1), Version(1, 4)]
print(sorted(versions))
print(Version(1, 4) < Version(2))
let names = {Version(1): "first"}
print(names[Version(1, 0)])
```

```output
[Version(major=1, minor=0), Version(major=1, minor=4), Version(major=2, minor=1)]
True
first
```

```taipan
@dataclass(frozen=True)
class Version:
    major: int

let v = Version(1)
v.major = 2
```

```error
main.tpn:6:3: error TP0202: cannot assign to 'major': 'Version' is frozen
```

```taipan
@dataclass
class Point:
    x: int

let seen = {Point(1): True}
```

```error
main.tpn:5:13: error TP0200: dict keys must be hashable, found Point
  help: a dataclass needs its own '__hash__' to be a key
```

## Inheritance

`class Dog(Animal):` declares `Dog` as a subclass of `Animal`, its base class. A `Dog` has every field and method of an `Animal`, and can be used wherever an `Animal` is expected.

```taipan
class Animal:
    name: str

    def __init__(self, name: str):
        self.name = name

    def speak(self) -> str:
        return "..."

    def introduce(self) -> str:
        return f"{self.name} says {self.speak()}"

class Dog(Animal):
    breed: str

    def __init__(self, name: str, breed: str):
        self.breed = breed
        super().__init__(name)

    @override
    def speak(self) -> str:
        return "woof"

let pets: list[Animal] = [Animal("generic"), Dog("Rex", "collie")]
for pet in pets:
    print(pet.introduce())
```

```output
generic says ...
Rex says woof
```

- A method call runs the method of the object's actual class, even through a variable of the base class's type. So `introduce`, written in `Animal`, calls `Dog.speak` for a `Dog`.
- A class has at most one base class. The base can't be an enum or a built-in type such as `int`. It may come from another module: `class Dog(zoo.Animal):`.
- A dataclass derives only from a dataclass, and a plain class only from a plain class.
- A subclass can't declare a field with the name of an inherited field or method.
- A subclass without its own `__init__` uses its base class's.

### `super().__init__(…)`: fields first, then the base

A subclass's `__init__` must call `super().__init__(…)` exactly once, directly in its body (not inside an `if` or a loop), **after** setting its own fields. That holds even when the base class has no `__init__` of its own, in which case the call takes no arguments. The base's `__init__` may call methods that the subclass overrides, and those may read the subclass's fields, so they must already be set:

```taipan
class Animal:
    name: str

    def __init__(self, name: str):
        self.name = name

class Dog(Animal):
    breed: str

    def __init__(self, name: str, breed: str):
        super().__init__(name)
        self.breed = breed
```

```error
main.tpn:11:9: error TP0226: 'super().__init__' is called before 'breed' is set
  help: set 'self.breed' first; the base '__init__' may call a method that reads it
```

`super().method(…)` calls the base class's version of any method, from any method of the subclass:

```taipan
class Animal:
    def speak(self) -> str:
        return "..."

class Puppy(Animal):
    @override
    def speak(self) -> str:
        return super().speak() + " yip"

print(Puppy().speak())
```

```output
... yip
```

### Overriding: `@override`

A method with the name of an inherited method replaces it for the subclass, which is called overriding. The overriding method must be marked `@override`. This catches a misspelled method name, which would otherwise quietly add a new method, and a method added to the base class later that would quietly take over the subclass's:

```taipan
class Animal:
    def speak(self) -> str:
        return "..."

class Cat(Animal):
    def speak(self) -> str:
        return "meow"

class Cow(Animal):
    @override
    def speek(self) -> str:
        return "moo"
```

```error
main.tpn:6:9: error TP0227: 'speak' overrides 'Animal.speak'; mark it '@override'
  help: write '@override' on the line above it, or rename it if it isn't meant to override
main.tpn:11:9: error TP0227: 'speek' does not override a method of 'Animal'
  help: remove '@override', or check the name against the base class's methods
```

An override must accept every call the base method accepts, so that code holding an `Animal` can call it safely:

- the same parameters, with the same names, kinds and types, in the same order
- a default wherever the base method has one (it may add more)
- the same result type, or, when the results are classes, a subclass of the base's result type

`__init__` doesn't override: each class has its own, with any parameters.

```taipan
class Animal:
    def speak(self, times: int) -> str:
        return "..." * times

class Dog(Animal):
    @override
    def speak(self, times: float) -> str:
        return "woof"
```

```error
main.tpn:7:21: error TP0228: parameter 'times' of 'speak' must have type int, as in 'Animal.speak'
```

### Checking and converting the class

| Form | Gives |
|---|---|
| `isinstance(x, Dog)` | `True` if `x` is a `Dog` or a subclass of it; in an `if`, `x` is a `Dog` inside the branch |
| `x as Dog` | `x` as a `Dog`; stops the program with an error if it isn't one |
| `x as Dog?` | `x` as a `Dog`, or `None` if it isn't one |
| `match x:` with `Dog(name=n):` | an arm for `Dog`s, binding fields; see [match](../match/) |

```taipan
class Animal:
    name: str

    def __init__(self, name: str):
        self.name = name

class Dog(Animal):
    def fetch(self) -> str:
        return f"{self.name} fetches"

def act(a: Animal) -> str:
    if isinstance(a, Dog):
        return a.fetch()
    return f"{a.name} ignores you"

let pets: list[Animal] = [Dog("Rex"), Animal("Tom")]
for p in pets:
    print(act(p), p as Dog?)
```

```output
Rex fetches <Dog object>
Tom ignores you None
```

## Properties: `@property`

`@property` makes a method that's read like a field, without parentheses. Its value is computed each time it's read. A method marked `@<name>.setter` directly after it lets the property be assigned:

```taipan
class Temperature:
    celsius: float

    def __init__(self, celsius: float):
        self.celsius = celsius

    @property
    def fahrenheit(self) -> float:
        return self.celsius * 9 / 5 + 32

    @fahrenheit.setter
    def fahrenheit(self, value: float):
        self.celsius = (value - 32) * 5 / 9

let t = Temperature(100)
print(t.fahrenheit)
t.fahrenheit = 32
print(t.celsius)
```

```output
212.0
0.0
```

- A property takes only `self` and has a result type. Its setter takes one parameter of the property's type and returns nothing.
- Without a setter, assigning the property is an error.
- A property may give a different value each time it's read, so an `if t.reading is not None:` check doesn't make `t.reading` safe to use as non-optional later. Copy it to a `let` first.

```taipan
class Circle:
    r: float

    def __init__(self, r: float):
        self.r = r

    @property
    def area(self) -> float:
        return 3.14159 * self.r * self.r

let c = Circle(1)
c.area = 2.0
```

```error
main.tpn:12:3: error TP0202: cannot assign to 'area': it is a read-only property
```

## Static methods, class methods and class attributes

| Kind | Declared as | Called as |
|---|---|---|
| static method | `@staticmethod` on a method without `self` | `C.m(…)` or `x.m(…)` |
| class method | `@classmethod` on a method whose first parameter is `cls` | `C.m(…)` or `x.m(…)`; `cls` is the class it was called through |
| class attribute | `count: ClassVar[int] = 0` in the class body | read as `C.count` or `x.count`; assigned only as `C.count = …` |

A class attribute is one value shared by the class and all its instances, where a field is one per instance. Its value must be a constant: literals, arithmetic on them, `None`, and lists, dicts and tuples of those.

```taipan
class Money:
    cents: int
    made: ClassVar[int] = 0

    def __init__(self, cents: int):
        self.cents = cents
        Money.made += 1

    @staticmethod
    def zero() -> Money:
        return Money(0)

    @classmethod
    def parse(cls, text: str) -> Money:
        return cls(int(text) ?? 0)

    def __str__(self) -> str:
        return f"${self.cents / 100:.2f}"

print(Money.parse("250"), Money.zero(), Money.parse("oops"))
print(Money.made)
```

```output
$2.50 $0.00 $0.00
3
```

- In a class method, `cls(…)` constructs an instance of the class it was called through, so a subclass calling an inherited class method gets an instance of the subclass. Every class derived from it must then take the same constructor arguments.
- `cls` can only be called, or used to reach static methods, class methods and class attributes: `cls.other()`, `cls.made`.
- Calling an ordinary method through the class instead of an instance, as in `Account.describe()`, is an error, since the method needs an instance to be its `self`.
- Static and class methods can be used as function values without calling them: `let p = Money.parse`.

## Abstract classes: `ABC` and `@abstractmethod`

A class derived from `ABC` may mark methods `@abstractmethod`, with `...` as their body. Such a method has no code of its own: each subclass must override it. A class with an abstract method that isn't overridden can't be constructed:

```taipan
class Shape(ABC):
    @abstractmethod
    def area(self) -> float: ...

    def describe(self) -> str:
        return f"area {self.area():.1f}"

class Square(Shape):
    side: float

    def __init__(self, side: float):
        self.side = side
        super().__init__()

    @override
    def area(self) -> float:
        return self.side * self.side

let shapes: list[Shape] = [Square(2)]
print(shapes[0].describe())
```

```output
area 4.0
```

```taipan
class Shape(ABC):
    @abstractmethod
    def area(self) -> float: ...

class Blob(Shape):
    pass

let b = Blob()
```

```error
main.tpn:8:9: error TP0234: cannot instantiate abstract class 'Blob'
  help: it does not implement 'Shape.area'
```

`ABC` doesn't count as a base class, so `class Square(Base, ABC):` is valid. `ABC`, `abstractmethod` and the other decorator names below are built in and need no import.

## `@final`

`@final` on a class means no class may derive from it. On a method, it means no subclass may override it.

```taipan
@final
class Config:
    pass

class Special(Config):
    pass
```

```error
main.tpn:5:15: error TP0225: cannot derive from final class 'Config'
```

## `Self`

In a method's signature, `Self` stands for the class of the object the method is called on. A method declared `-> Self` and inherited by a subclass returns the subclass's type:

```taipan
class Node:
    label: str

    def __init__(self, label: str):
        self.label = label

    def tagged(self, tag: str) -> Self:
        self.label = f"{self.label} [{tag}]"
        return self

class Leaf(Node):
    def is_leaf(self) -> bool:
        return True

let leaf = Leaf("a").tagged("x")
print(leaf.label, leaf.is_leaf())
```

```output
a [x] True
```

A method returning `Self` must return `self`, `cls(…)` in a class method, or another `Self` value. Returning a new `Node(…)` would be an error, since for a `Leaf` it would be the wrong class.

## Special methods

A special method has a name with two underscores on each side, such as `__str__`. You don't call it yourself: Taipan calls it for an operator or a built-in function. Each must have exactly the parameters and result shown, and any other name of the form `__x__` is an error.

| Method | Signature | Used by |
|---|---|---|
| `__init__` | `(self, …)` | constructing: `C(…)` |
| `__str__` | `(self) -> str` | `print`, `str(x)`, f-strings |
| `__repr__` | `(self) -> str` | `repr(x)`, `!r` in f-strings, and display inside lists, dicts, tuples and dataclasses; also `print` when there's no `__str__` |
| `__eq__` | `(self, other: C) -> bool`, `C` the class or a base | `==`, `!=`, `in`, and equality of lists and dicts holding it |
| `__hash__` | `(self) -> int` | dict keys, set elements, `hash(x)` |
| `__lt__`, `__le__`, `__gt__`, `__ge__` | `(self, other: T) -> bool` | `<`, `<=`, `>`, `>=`; `sorted` and `sort` use `__lt__` |
| `__add__`, `__sub__`, `__mul__`, `__truediv__`, `__floordiv__`, `__mod__`, `__pow__` | `(self, other: T) -> R` | `+`, `-`, `*`, `/`, `//`, `%`, `**` with the object on the left |
| `__radd__`, `__rsub__`, `__rmul__`, `__rtruediv__`, `__rfloordiv__`, `__rmod__`, `__rpow__` | `(self, other: T) -> R` | the same operators with the object on the right |
| `__iadd__`, `__isub__`, `__imul__`, `__itruediv__`, `__ifloordiv__`, `__imod__`, `__ipow__` | `(self, other: T) -> R` | `+=` and the others; without one, `x += y` is `x = x + y` |
| `__neg__` | `(self) -> R` | `-x` |
| `__len__` | `(self) -> int` | `len(x)` |
| `__getitem__` | `(self, key: K) -> V` | `x[k]` |
| `__setitem__` | `(self, key: K, value: V)` | `x[k] = v` |
| `__contains__` | `(self, item: T) -> bool` | `in`, `not in` |
| `__call__` | `(self, …) -> R` | `x(…)`, and using `x` as a function value |
| `__format__` | `(self, spec: str) -> str` | f-string holes with a format spec: `f"{x:short}"` |
| `__iter__` | `(self) -> Iterator[T]` | `for`, and built-ins that take an iterable; see [Iterators and generators](../generators/) |
| `__next__` | `(self) -> T` | `next(x)` |
| `__enter__`, `__exit__` | see [with](../with/) | `with` |
| `__aiter__`, `__anext__`, `__aenter__`, `__aexit__` | see [async and await](../async/) | `async for`, `async with` |

```taipan
@dataclass(frozen=True)
class Money:
    cents: int

    def __add__(self, other: Money) -> Money:
        return Money(self.cents + other.cents)

    def __radd__(self, other: int) -> Money:
        return Money(self.cents + other)

    def __lt__(self, other: Money) -> bool:
        return self.cents < other.cents

    def __str__(self) -> str:
        return f"${self.cents / 100:.2f}"

let prices = [Money(250), Money(99), Money(1000)]
print(prices[0] + prices[1], 5 + prices[2])
print(min(prices), sorted(prices))
```

```output
$3.49 $10.05
$0.99 [Money(cents=99), Money(cents=250), Money(cents=1000)]
```

`prices[0] + prices[1]` uses `__add__`, and `5 + prices[2]`, with the `Money` on the right, uses `__radd__`. Inside a list, values display with `__repr__`, which for a dataclass is the generated `Money(cents=99)` form.

Special methods can't be called by name: `m.__str__()` is an error whose help names the form to use instead.

```taipan
class Box:
    def __str__(self) -> str:
        return "box"

print(Box().__str__())
```

```error
main.tpn:5:13: error TP0200: '__str__' cannot be called directly
  help: use 'str(Box())'
```

### Equality and hashing

- A class that defines `__eq__` without `__hash__` can't be a dict key or set element, since equal objects must hash equally and Taipan can't know how. Define `__hash__` too.
- A dataclass, which generates its own `__eq__`, needs its own `__hash__` to be a key, unless it's `frozen=True`.
- A subclass of a class that can be a key must be able to be one too.

```taipan
class Point:
    x: int

    def __init__(self, x: int):
        self.x = x

    def __eq__(self, other: Point) -> bool:
        return self.x == other.x

let seen = {Point(1): True}
```

```error
main.tpn:10:13: error TP0200: dict keys must be hashable, found Point
  help: 'Point' defines '__eq__' without '__hash__'
```

### Ordering and `@total_ordering`

`<=` needs `__le__`, and `>=` needs `__ge__`: defining `__lt__` gives only `<` and `>`. `@total_ordering` on a class that defines one of the four generates the others from it and `==`:

```taipan
@total_ordering
class Grade:
    score: int

    def __init__(self, score: int):
        self.score = score

    def __eq__(self, other: Grade) -> bool:
        return self.score == other.score

    def __lt__(self, other: Grade) -> bool:
        return self.score < other.score

print(Grade(80) <= Grade(80), Grade(90) >= Grade(70), Grade(60) > Grade(70))
```

```output
True True False
```

### Containers and calls

```taipan
class Grid:
    cells: dict[tuple[int, int], str]

    def __init__(self):
        self.cells = {}

    def __len__(self) -> int:
        return len(self.cells)

    def __getitem__(self, at: tuple[int, int]) -> str:
        return self.cells.get(at, ".")

    def __setitem__(self, at: tuple[int, int], value: str):
        self.cells[at] = value

    def __contains__(self, at: tuple[int, int]) -> bool:
        return at in self.cells

    def __call__(self, row: int) -> str:
        return "".join([self[(row, c)] for c in range(3)])

let g = Grid()
g[(0, 1)] = "x"
print(len(g), g[(0, 1)], (2, 2) in g)
print(g(0))
```

```output
1 x False
.x.
```

## Generic classes

A class can take type parameters, written in brackets after its name, so that one class works for many element types: `class Box[T]:`. See [Types](../types/#generics).

## Related pages

- Protocols, which describe what methods a type has without inheritance: [Types](../types/#protocols).
- Exception classes, derived from `Exception`: [Errors](../errors/).
- Class patterns in `match`: [match](../match/).
