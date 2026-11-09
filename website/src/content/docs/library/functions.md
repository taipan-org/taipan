---
title: Built-in functions
description: "Every built-in function: output and input, numbers, sequences and iterating, converting, and classes."
sidebar:
  order: 2
---

Built-in functions can be called from anywhere, without an `import`. Each one checks its arguments when the program is compiled, so passing the wrong kind of value is a compile error, not a crash when the line runs.

## Output and input

| Call | Gives |
|---|---|
| `print(a, b, …)` | nothing; writes each value's text, with a space between each and a newline at the end |
| `print(…, sep=s)` | the same, with `s` between the values instead of a space |
| `print(…, end=s)` | the same, with `s` at the end instead of a newline |
| `print(…, file=sys.stderr)` | the same, written to the error stream instead of the screen; needs `import sys` |
| `print(…, flush=True)` | the same, sent out at once rather than when the buffer fills |
| `print()` | nothing; writes an empty line |
| `input()` | a `str`: one line typed by the user, without its line ending; raises `EOFError` when there's nothing left to read |
| `input(prompt)` | the same, after writing `prompt` with no newline after it |
| `open(path)`, `open(path, mode)` | a `TextIO`, an open text file; see [Files](../files/) |

`sep`, `end`, `file` and `flush` must be given by name, and can be combined. Each value is written as `str` would write it.

```taipan
print("total:", 3, True)
print("a", "b", "c", sep=", ")
print("loading", end="... ")
print("done")
```

```output
total: 3 True
a, b, c
loading... done
```

`input` waits for the user to type a line and press Enter:

```taipan
let name = input("Your name? ")
print(f"Hello, {name}!")
```

If the user types `Ada`, the screen shows `Your name? Ada` and then `Hello, Ada!`. To read a number, pass the text to `int` or `float` (see [Converting](#converting)).

## Numbers

| Call | Gives |
|---|---|
| `abs(x)` | `x` without its sign: an `int` for an `int`, a `float` for a `float`; raises `OverflowError` for the smallest `int` |
| `round(x)` | the nearest `int`; a value exactly halfway goes to the even one, so `round(2.5)` is `2`; raises `ValueError` for NaN and `OverflowError` for infinity |
| `round(x, n)` | `x` rounded to `n` digits after the point, as a `float` for a `float`; for an `int`, a negative `n` rounds to tens, hundreds and so on |
| `divmod(a, b)` | the tuple `(a // b, a % b)`; raises `ZeroDivisionError` when `b` is zero |
| `sum(xs)` | the total of a list or other iterable of numbers: an `int` for `int`s, a `float` if any is a `float` |
| `sum(xs, start)` | the same, added to `start` instead of `0` |
| `min(a, b, …)`, `max(a, b, …)` | the smallest or largest of two or more values |
| `min(xs)`, `max(xs)` | the smallest or largest value in a list or other iterable; raises `ValueError` if it's empty |
| `min(xs, default=d)`, `max(xs, default=d)` | the same, but `d` if it's empty |
| `min(…, key=f)`, `max(…, key=f)` | the value for which `f` gives the smallest or largest result |

`min` and `max` take numbers, strings, tuples of those, and objects that can be compared with `<`. When values are equal, the first one wins. When `int`s and `float`s are mixed, the result is a `float`, so `max(3, 2.5)` is `3.0`. `sum` adds `float`s carefully, so ten `0.1`s make exactly `1.0`. `sum` of strings is a compile error: use `"".join(xs)` (see [str](../str/)).

```taipan
print(abs(-7), abs(-2.5))
print(round(2.5), round(3.5), round(3.14159, 2), round(1234, -2))
print(divmod(17, 5), divmod(-17, 5))
print(sum([1, 2, 3]), sum([0.1] * 10), sum([1, 2], 100))
print(min(4, 2, 8), max([4, 2, 8]), max(3, 2.5))
print(min(["pear", "fig", "banana"], key=len), max([1, 2], default=0))
```

```output
7 2.5
2 4 3.14 1200
(3, 2) (-4, 3)
6 1.0 103
2 8 3.0
fig 2
```

The operators on numbers, and how `int` and `float` mix, are on the [int, float and bool](../numbers/) page.

## Sequences and iterating

Several of these give an *iterator*: a stream that produces its values one at a time, as they're asked for, and can be gone through only once. A `for` loop can go through an iterator directly, and `list(…)` collects one into a list. See [Iterators and generators](../../tour/iterators-and-generators/).

| Call | Gives |
|---|---|
| `len(x)` | an `int`: the number of items in a list, dict, set or `range`, or of characters in a `str` |
| `range(stop)` | the whole numbers `0`, `1`, … up to, not including, `stop` |
| `range(start, stop)` | the whole numbers from `start` up to, not including, `stop` |
| `range(start, stop, step)` | the same, counting by `step`; a negative `step` counts down; raises `ValueError` when `step` is `0` |
| `sorted(xs)` | a new list of the values in `xs`, smallest first |
| `sorted(xs, key=f)` | the same, ordered by what `f` gives for each value |
| `sorted(xs, reverse=True)` | the same, largest first |
| `reversed(xs)` | an iterator over a list, `str`, dict's keys, `range` or `tuple[T, ...]`, from the last item to the first |
| `enumerate(xs)` | an iterator of `(index, value)` tuples, counting from `0` |
| `enumerate(xs, start)` | the same, counting from `start`; also written `start=n` |
| `zip(xs, ys, …)` | an iterator of tuples, one value from each of two or more iterables, stopping at the shortest |
| `map(f, xs)` | an iterator of `f(x)` for each value `x` in `xs` |
| `filter(f, xs)` | an iterator of the values in `xs` for which `f` gives `True` |
| `any(xs)` | `True` if any value in an iterable of `bool`s is `True`; stops at the first `True` |
| `all(xs)` | `True` if every value is `True`; stops at the first `False` |
| `iter(xs)` | an iterator over `xs`; for an iterator, the iterator itself |
| `next(it)` | the next value from the iterator `it`; raises `StopIteration` when there's none |
| `next(it, default)` | the same, but `default` when there's none |
| `anext(it)`, `anext(it, default)` | the same for an async iterator, to be used with `await`; raises `StopAsyncIteration` |

A `range` holds no list of numbers, only its start, stop and step, so `range(1_000_000)` takes no memory to speak of. It can be looped over many times, and `x in r`, `r[i]` and `len(r)` work on it. `len` of an iterator is a compile error, since its length isn't known until it's been gone through: convert it with `list(…)` first.

`sorted` keeps values that compare equal in their original order. Its `key` and `reverse` must be given by name. `any` and `all` take only `bool`s, since Taipan doesn't treat other values as true or false: write `any(x > 0 for x in xs)` to test each value.

```taipan
let words = ["pear", "fig", "banana"]
print(len(words), len("héllo"))
print(list(range(4)), list(range(2, 10, 3)), list(range(3, 0, -1)))
print(sorted(words), sorted(words, key=len), sorted(words, reverse=True))
print(list(reversed(words)))
for i, word in enumerate(words, start=1):
    print(i, word)
for word, price in zip(words, [3, 5, 2]):
    print(word, price)
```

```output
3 5
[0, 1, 2, 3] [2, 5, 8] [3, 2, 1]
["banana", "fig", "pear"] ["fig", "pear", "banana"] ["pear", "fig", "banana"]
["banana", "fig", "pear"]
1 pear
2 fig
3 banana
pear 3
fig 5
banana 2
```

`map` and `filter` call their function only when the next value is asked for. `next` takes values from an iterator one at a time, and a `for` loop takes the rest:

```taipan
let numbers = [3, 8, 1, 6]
print(list(map(lambda n: n * 10, numbers)))
print(list(filter(lambda n: n > 2, numbers)))
print(any(n > 7 for n in numbers), all(n > 0 for n in numbers))
let it = iter(numbers)
print(next(it), next(it))
for n in it:
    print(n)
print(next(it, -1))
```

```output
[30, 80, 10, 60]
[3, 8, 6]
True True
3 8
1
6
-1
```

`anext` is for async iterators, such as an `async def` that uses `yield` (see [Async](../../tour/async/)):

```taipan
import asyncio

async def count(n: int) -> AsyncIterator[int]:
    for i in range(n):
        yield i

async def main():
    let it = count(2)
    print(await anext(it), await anext(it), await anext(it, -1))

asyncio.run(main())
```

```output
0 1 -1
```

## Converting

| Call | Gives |
|---|---|
| `str(x)` | the text `print` would write for any value |
| `repr(x)` | the text a value has inside a list: a `str` in double quotes, with special characters such as tabs and newlines written as `\t` and `\n` |
| `int(x)` | for a `float`, the whole number part, dropping anything after the point; raises `OverflowError` if it's too big for an `int`, or infinite, and `ValueError` for NaN |
| `int(s)` | for a `str`, an `int?`: the number the text spells, or `None` if it isn't one |
| `float(x)` | for an `int`, the same number as a `float` |
| `float(s)` | for a `str`, a `float?`: the number the text spells, or `None` if it isn't one |
| `ord(c)` | an `int`: the Unicode code point of a one-character `str`; raises `TypeError` for any other length |
| `chr(n)` | a one-character `str`: the character with code point `n`; raises `ValueError` outside `0` to `1114111` |
| `hash(x)` | an `int` that's equal for equal values; only for values that can be dict keys |
| `list(xs)` | a new list of every value in an iterable, in order |
| `set(xs)` | a new set of the distinct values in an iterable |
| `dict(pairs)` | a new dict from an iterable of `(key, value)` tuples |
| `dict(d)` | a copy of the dict `d` |
| `dict(a=1, b=2)` | a new `dict[str, …]` from names and values |
| `tuple(xs)` | a new tuple of every value in an iterable |
| `list()`, `set()`, `dict()` | a new empty one, whose type has to be written, as in `let s: set[int] = set()` |

`int(s)` accepts an optional `+` or `-`, then digits, with `_` allowed between digits, and spaces around them. `float(s)` also accepts a fraction and an exponent (`2.5`, `.5`, `1e3`, `2e-3`), and `inf`, `infinity` and `nan` in any mix of capitals. Anything else gives `None`. Only the digits `0` to `9` count, and a number too big for an `int` gives `None` too.

Since the result of `int(s)` or `float(s)` is an optional, it has to be checked before it's used as a number: give a default with `??`, or test it with `is some` (see [Optionals](../../tour/optionals/)):

```taipan
let raw = "12x"
if int(raw) is some n:
    print(n * 2)
else:
    print(f"{raw!r} isn't a number")
```

```output
"12x" isn't a number
```

`int` and `float` don't take a `bool`, and there's no `bool(…)` function: write the comparison you mean, such as `n != 0` or `s != ""`.

```taipan
print(str(42) + "!", str([1, 2]), str(None))
print(repr("hi"), repr("a\tb"), repr(3))
print(int(3.9), int(-3.9), float(2))
print(int("42") ?? 0, int("4.2") ?? 0, float("2.5") ?? 0.0)
print(ord("A"), chr(97), chr(128013))
print(hash(10), hash("a") == hash("a"))
```

```output
42! [1, 2] None
"hi" "a\tb" 3
3 -3 2.0
42 0 2.5
65 a 🐍
10 True
```

`hash` of a `str` may differ from one run of the program to the next, so don't save it. The collection functions make new collections, so changing the result never changes the original:

```taipan
print(list("abc"), list(range(3)))
print(set([3, 1, 3, 2]), set("aab"))
print(dict([("a", 1), ("b", 2)]), dict(x=1, y=2))
print(tuple([1, 2, 3]), tuple("ab"))
let empty: set[int] = set()
print(empty)
```

```output
["a", "b", "c"] [0, 1, 2]
{3, 1, 2} {"a", "b"}
{"a": 1, "b": 2} {"x": 1, "y": 2}
(1, 2, 3) ("a", "b")
set()
```

A set keeps its values in the order they were first added. See [dict](../dict/) for more ways to make a dict.

## Types and classes

| Call | Gives |
|---|---|
| `isinstance(x, C)` | `True` if `x` is an object of the class `C`, or of a class derived from `C`; `False` for `None` |
| `isinstance(x, (A, B, …))` | `True` if `x` is an object of any of the classes listed |
| `super().m(…)` | calls the method `m` as the base class defines it, from inside a method of a derived class |

`isinstance` works on objects of classes, and `C` must be a class `x` could belong to, so `isinstance(1, int)`, or a test that could never be `True`, is a compile error. Inside an `if isinstance(x, C):`, `x` is known to be a `C`, so `C`'s fields and methods can be used on it. `super()` is only valid as `super().m(…)`. See [Classes](../../tour/classes/).

```taipan
class Animal:
    name: str

    def __init__(self, name: str):
        self.name = name

    def speak(self) -> str:
        return "..."

class Dog(Animal):
    @override
    def speak(self) -> str:
        return super().speak() + " woof"

    def fetch(self) -> str:
        return f"{self.name} fetches"

class Cat(Animal):
    pass

let pets: list[Animal] = [Dog("Rex"), Cat("Tom")]
for pet in pets:
    print(pet.name, pet.speak(), isinstance(pet, (Dog, Cat)))
    if isinstance(pet, Dog):
        print(pet.fetch())
```

```output
Rex ... woof True
Rex fetches
Tom ... True
```
