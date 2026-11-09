---
title: Iterators and generators
description: "Iterator and Iterable, generator functions with yield and yield from, Generator with send, throw and close, laziness, and iterator classes."
sidebar:
  order: 16
---

An *iterator* produces a sequence of values one at a time, each only when it's asked for. A *generator* is the easiest way to write one: a function that contains `yield`, which hands out a value and pauses the function until the next value is wanted. A `for` loop goes over any of them.

Iterators and generators are defined in [§3.13](../../reference/specification/#313-iterators-and-generators-m31) and [§3.20](../../reference/specification/#320-generators-and-async-ii-m42). For generators that wait for async work, see [async and await](../async/#async-generators).

## `Iterable[T]` and `Iterator[T]`

| Type | What it is | Going over it again |
|---|---|---|
| `Iterable[T]` | anything a `for` loop can go over: a `list[T]`, a `set[T]`, a `dict[T, V]` (its keys), a `str` (each character, as an `Iterable[str]`), a `range` (as `Iterable[int]`), an `Iterator[T]`, or an instance of a class with `__iter__` | a list, dict, string, range or class starts again from its beginning |
| `Iterator[T]` | a single-pass stream of `T` values: a generator, the result of `map`, `filter`, `zip`, `enumerate` and `iter`, an open file (its lines), or an instance of a class with `__next__` | continues from where it stopped; a value once taken is gone |

Every type in the first column's list is an `Iterable[T]`, so a parameter declared `Iterable[T]` accepts all of them. A `list[T]` isn't an `Iterator[T]`, though: `iter(xs)` starts one over it.

```taipan
let words = ["red", "green", "blue"]
let it = iter(words)
print(next(it))
for w in it:
    print("loop 1:", w)
for w in it:
    print("loop 2:", w)
for w in words:
    print("list:", w)
```

```output
red
loop 1: green
loop 1: blue
list: red
list: green
list: blue
```

The built-ins `iter(xs)` and `next(it)`:

- `iter(xs)` gives an iterator over any iterable. On an iterator, it gives the same iterator back.
- `next(it)` takes the next value, raising `StopIteration` when there's none. `next(it, default)` gives `default` instead.

An iterator can be used with `for`, unpacking, `in` and `not in` (which take values until one is equal), and every built-in that takes an iterable, such as `list`, `sum`, `sorted` and `zip`. It can't be indexed, sliced, measured with `len` or used as a dict key, since it doesn't know its values in advance:

```taipan
let squares = map(lambda x: x * x, [1, 2, 3])
print(len(squares))
```

```error
main.tpn:2:11: error TP0200: 'len' takes a list, dict or str, found Iterator[int]
  help: convert it with 'list(…)' first
```

`==` and `!=` on iterators compare identity. They display as `<generator object f>`, `<map object>`, `<filter object>`, `<zip object>`, `<enumerate object>`, `<list_iterator object>` and `<iterator object>`.

## Generator functions

A function whose body contains `yield` is a *generator function*. It must be declared to return `Iterator[T]`, `Iterable[T]` or `Generator[Y, S, R]` (below):

```taipan
def countdown(n: int) -> Iterator[int]:
    var i = n
    while i > 0:
        yield i
        i -= 1

for x in countdown(3):
    print(x)
print(list(countdown(2)))
```

```output
3
2
1
[2, 1]
```

Any other return type, or none, is `TP0200`:

```taipan
def countdown(n: int) -> list[int]:
    yield n
```

```error
main.tpn:1:5: error TP0200: 'countdown' yields, so it must return 'Iterator[T]'
```

The rules:

- `yield e` hands `e` to whatever asked for the next value. `e` is checked against `T`.
- `return` without a value, or reaching the end of the body, ends the generator. Since a generator never has to return, a missing `return` is not an error.
- A `yield` inside a nested function makes that function a generator, not the outer one. A lambda can't contain `yield`, and `yield` outside a function is `TP0215`.
- Methods can be generators, `__iter__` included.
- `yield` may appear anywhere in the body, including inside `try`, `except`, `finally` and `with` blocks.
- A `StopIteration` raised inside a generator and not caught there becomes a `RuntimeError`, `generator raised StopIteration`.

### Laziness

Calling a generator function runs none of its body. It gives a new generator; the body runs up to the first `yield` when the first value is asked for, then pauses until the next one is asked for, and so on. The arguments, and any defaults, are evaluated at the call.

```taipan
def numbers() -> Iterator[int]:
    print("starting")
    yield 1
    print("after 1")
    yield 2
    print("finished")

let g = numbers()
print("made", g)
print(next(g))
print(next(g))
print(next(g, -1))
```

```output
made <generator object numbers>
starting
1
after 1
2
finished
-1
```

Because values are made only on demand, a generator can be endless, and the program takes only as many values as it needs:

```taipan
def naturals(start: int) -> Iterator[int]:
    var n = start
    while True:
        yield n
        n += 1

for i, name in zip(naturals(1), ["a", "b", "c"]):
    print(i, name)
```

```output
1 a
2 b
3 c
```

A `for` loop that stops early, through `break`, `return` or an exception, leaves the generator where it is, and a later loop or `next` continues from there:

```taipan
def letters() -> Iterator[str]:
    yield "a"
    yield "b"
    yield "c"

let g = letters()
for x in g:
    print("first loop:", x)
    break
for x in g:
    print("second loop:", x)
```

```output
first loop: a
second loop: b
second loop: c
```

A `finally` block around a `yield` runs when the generator finishes or is closed (below). A generator that's never finished or closed never runs it.

### `yield from`

`yield from e` hands out each value of the iterable `e` in turn, as a loop with a `yield` inside would. `e`'s values must fit the generator's `T`.

```taipan
def flatten(rows: list[list[int]]) -> Iterator[int]:
    for row in rows:
        yield from row

print(list(flatten([[1, 2], [3], []])))
```

```output
[1, 2, 3]
```

When `e` is itself a `Generator`, `yield from e` also passes along everything sent or thrown into the outer generator, and gives `e`'s return value (see below).

## `Generator[Y, S, R]`: two-way generators

A generator can also receive values while it runs, and return a final value. Its type is then `Generator[Y, S, R]`: it yields `Y`s, receives `S`s, and returns an `R`. Any of the three may be `None`. `Iterator[Y]` is short for a generator that receives and returns `None`, and a `Generator[Y, S, R]` can be used wherever an `Iterator[Y]` or `Iterable[Y]` is expected.

- **`yield` as an expression.** `let x = yield e` hands out `e`, pauses, and when resumed gives `x` the value it was resumed with, an `S`. Outside parentheses, a `yield` may only be a whole statement or the right side of `let`, `var` or an assignment (`TP0100`); elsewhere write `(yield e)`. A bare `yield`, without a value, yields `None`, so `Y` must accept `None`.
- **`g.send(v)`** resumes the generator with `v` and gives the next value it yields. `next(g)` and a `for` loop resume it with `None`.
- **`return e`** ends the generator with the value `e`, checked against `R`. It becomes the `value` of the `StopIteration` that ends the generator, and the value of a `yield from` over it.

```taipan
def averager() -> Generator[float, float, int]:
    var total = 0.0
    var count = 0
    var average = 0.0
    while count < 3:
        let value = yield average
        total += value
        count += 1
        average = total / count
    return count

let avg = averager()
next(avg)
print(avg.send(10.0), avg.send(20.0))
try:
    avg.send(30.0)
except StopIteration as e:
    print("returned", e.value)
```

```output
10.0 15.0
returned 3
```

`StopIteration.value` has the type `object`, which can hold any value, since an exception class can't depend on the generator's `R`. Convert it with `as`: `e.value as int` gives the `int`, and raises `TypeError` if it's something else.

A generator that's declared `Iterator` or `Iterable` can't return a value:

```taipan
def count() -> Iterator[int]:
    yield 1
    return 1
```

```error
main.tpn:3:12: error TP0200: a generator declared 'Iterator[int]' can't return a value
  help: declare it 'Generator[Y, S, R]'
```

A generator has to be started with `next(g)` before `send` can give it a value, since it isn't yet paused at a `yield` that could receive one:

```taipan
def echo() -> Generator[str, str, None]:
    while True:
        let got = yield "ready"
        print("got", got)

let g = echo()
g.send("hello")
```

```error
Traceback (most recent call last):
  File "main.tpn", line 7, in <module>
    g.send("hello")
TypeError: can't send non-None value to a just-started generator
```

And a generator whose `yield` needs a value that can't be `None` must be resumed with `send`, not `next`:

```taipan
def echo() -> Generator[str, str, None]:
    while True:
        let got = yield "ready"
        print("got", got)

let g = echo()
print(next(g))
print(g.send("hello"))
next(g)
```

```output
ready
got hello
ready
```

```error
Traceback (most recent call last):
  File "main.tpn", line 9, in <module>
    next(g)
TypeError: a generator waiting for a str can't be resumed with next(); use send()
```

The value of `yield from e` is `e`'s return value:

```taipan
def inner() -> Generator[int, None, str]:
    yield 1
    yield 2
    return "inner done"

def outer() -> Generator[int, None, None]:
    let result = yield from inner()
    print(result)
    yield 3

print(list(outer()))
```

```output
inner done
[1, 2, 3]
```

## `throw` and `close`

Two more methods of `Generator[Y, S, R]` raise an exception inside a paused generator, at its `yield`:

- **`g.throw(e)`** raises the exception `e` there, and gives the next value the generator yields if it catches `e` and goes on. If it doesn't catch it, `e` comes out of `throw`.
- **`g.close()`** raises `GeneratorExit` there, so the generator's `finally` blocks run. If the generator then yields again, `close` raises `RuntimeError`, `generator ignored GeneratorExit`. Closing a generator that hasn't started, or has finished, does nothing.

`GeneratorExit` derives from `BaseException`, not `Exception`, so an `except Exception` in the generator doesn't catch it.

```taipan
def worker() -> Generator[str, None, None]:
    try:
        while True:
            try:
                yield "working"
            except ValueError as e:
                print("recovered from", e)
    finally:
        print("cleaned up")

let w = worker()
print(next(w))
print(w.throw(ValueError("a glitch")))
w.close()
print(next(w, "finished"))
```

```output
working
recovered from a glitch
working
cleaned up
finished
```

A finished generator gives no more values: `next` and `send` raise `StopIteration`, and `throw` raises the exception it was given. Resuming a generator from inside itself raises `ValueError`, `generator already executing`.

## Iterator classes

A class can be iterable without being a generator, by defining special methods:

- **`__iter__(self) -> Iterator[T]`** makes the class an `Iterable[T]`. A `for` loop over an instance calls it once and takes values from what it returns. It's often a generator method.
- **`__next__(self) -> T`** makes the class an `Iterator[T]`. Each value asked for is a call to `__next__`, and the iteration ends when it raises `StopIteration`. A class with `__next__` is its own iterator.

If a class has both, they must agree on `T` (`TP0225`). Calling either method directly is `TP0200`; use `iter(x)` and `next(x)`.

```taipan
class Deck:
    cards: list[str]

    def __init__(self, cards: list[str]):
        self.cards = cards

    def __iter__(self) -> Iterator[str]:
        for card in self.cards:
            yield card

class Countdown:
    n: int

    def __init__(self, n: int):
        self.n = n

    def __next__(self) -> int:
        if self.n == 0:
            raise StopIteration()
        self.n -= 1
        return self.n + 1

let deck = Deck(["ace", "king"])
for c in deck:
    print(c)
print(list(deck))
print(list(Countdown(3)))
```

```output
ace
king
["ace", "king"]
[3, 2, 1]
```

```taipan
class Countdown:
    n: int

    def __init__(self, n: int):
        self.n = n

    def __next__(self) -> int:
        if self.n == 0:
            raise StopIteration()
        self.n -= 1
        return self.n + 1

let c = Countdown(2)
print(c.__next__())
```

```error
main.tpn:14:9: error TP0200: '__next__' cannot be called directly
  help: use 'next(c)'
```
