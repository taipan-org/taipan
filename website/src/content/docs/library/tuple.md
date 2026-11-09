---
title: tuple
description: "Tuples: fixed groups of values, indexing, unpacking, comparing, returning several values, and tuples as dict keys."
sidebar:
  order: 8
---

A tuple is a group of values that belong together, such as a point's two coordinates or a name with its age. Each position has its own type, so `tuple[str, int]` holds a `str` and then an `int`. A tuple can't be changed once it's made: its elements can't be replaced, added or removed.

There are two kinds. A `tuple[A, B, …]` has a fixed number of elements, at least two, each of its own type. A `tuple[T, ...]`, with three dots, holds any number of elements, all of type `T` (see [Tuples of any length](#tuples-of-any-length)).

## Making and reading tuples

A tuple is written as values between parentheses, separated by commas. Its type is inferred, so it only needs writing for a function's parameters and return type, or when a literal should have a wider type, as `let p: tuple[float, float] = (1, 2)` makes `(1.0, 2.0)`.

| Operation | Gives |
|---|---|
| `(a, b)`, `(a, b, c)` | a tuple of those values |
| `t[0]`, `t[1]` | the element at that position, counting from 0 |
| `t[-1]` | the last element; negative positions count from the end |
| `t == other`, `t != other` | whether both have equal elements, position by position |
| `<`, `<=`, `>`, `>=` | comparison, first elements first (see [Comparing](#comparing)) |

The position in `t[i]` must be written as a number, such as `t[1]` or `t[-1]`, so that the compiler knows the element's type, and a position past the end is an error. A tuple can't be looped over, sliced, measured with `len` or searched with `in`: use a list, or a `tuple[T, ...]`, for that. `t[0] = x` is an error, since tuples can't be changed.

`(x,)`, with one element, and `()`, with none, are errors: a group of one is just the value itself.

```taipan
let point = (3, 4)
let entry: tuple[str, int] = ("ada", 36)
print(point, entry)
print(entry[0], entry[1], entry[-1])
```

```output
(3, 4) ("ada", 36)
ada 36 36
```

## Returning several values

A function returns several values by returning a tuple. The parentheses may be left out after `return`, and unpacking (below) gives each value its own name:

```taipan
def bounds(xs: list[int]) -> tuple[int, int]:
    return min(xs), max(xs)

let lo, hi = bounds([4, 1, 9])
print(lo, hi)
let b = bounds([5, 2])
print(b)
```

```output
1 9
(2, 5)
```

## Unpacking

Unpacking takes a tuple apart into one name for each element: `let lo, hi = pair`. It works in `let` and `var`, in plain assignments, and in `for` loops.

| Form | Does |
|---|---|
| `let a, b = t` | declares `a` and `b` with `t`'s two elements; each gets its element's type |
| `a, b = b, a` | assigns to existing names; the right side is worked out first, so this swaps them |
| `let name, _ = t` | `_` ignores an element, and may appear more than once |
| `let (a, b), c = t` | parentheses unpack an element that's itself a tuple |
| `for k, v in d.items():` | unpacks each element a loop goes over |
| `let first, *rest = xs` | a name with `*` takes all the elements the others don't, as a new list |

A tuple must have exactly as many elements as there are names, or it's an error. A list can be unpacked too, and its length is checked when the program runs: too few or too many elements raise `ValueError`.

A starred name may appear once on each level, anywhere among the names, and can be `*_` to ignore the elements it takes. It always gives a `list`, even when the elements came from a tuple.

```taipan
var a = 1
var b = 2
a, b = b, a
print(a, b)
let name, _ = ("ada", 36)
print(name)
let (x, y), z = ((1, 2), 3)
print(x, y, z)
for i, (k, v) in enumerate({"a": 1, "b": 2}.items()):
    print(i, k, v)
```

```output
2 1
ada
1 2 3
0 a 1
1 b 2
```

```taipan
let first, *rest = [1, 2, 3, 4]
print(first, rest)
let head, *_, last = "a,b,c,d".split(",")
print(head, last)
let n, *more = (1, 2.5, 3)
print(n, more)
```

```output
1 [2, 3, 4]
a d
1 [2.5, 3.0]
```

In the last example, `more` is a `list[float]`, since the elements it takes are a `float` and an `int`, which mix to `float`.

## Comparing

`==` and `!=` compare tuples element by element. `<`, `<=`, `>` and `>=` compare the first elements, and only if those are equal the second, and so on, when every element is a number, a string, or another tuple of those. This makes tuples a good `key` for sorting by several things at once, as `sorted(people, key=lambda p: (p.age, p.name))` sorts by age, then by name (see [list](../list/#sorting)):

```taipan
print((1, 2) == (1, 2), (1, "b") < (1, "c"), (2, 0) > (1, 9))
print(sorted([(2, "b"), (1, "z"), (2, "a")]))
```

```output
True True True
[(1, "z"), (2, "a"), (2, "b")]
```

## As dict keys

A tuple whose elements could each be dict keys can be a dict key itself, or a set value, so a dict can be looked up by two things at once, such as a grid's row and column (see [dict](../dict/)):

```taipan
var grid: dict[tuple[int, int], str] = {}
grid[(0, 0)] = "start"
grid[(2, 1)] = "end"
print(grid[(2, 1)], (0, 0) in grid)
print(grid)
```

```output
end True
{(0, 0): "start", (2, 1): "end"}
```

A tuple that holds a list can't be a key, since the list could change.

## Tuples of any length

A `tuple[T, ...]` holds any number of `T`s, including none. It's what `tuple(xs)` gives for a collection of `T`s, and a tuple literal becomes one when that's the type expected. Unlike a fixed tuple, it can be used much as a list can, except that it can't be changed:

| Operation | Gives |
|---|---|
| `tuple(xs)` | a `tuple[T, ...]` of the values of any collection, in order |
| `t[i]` | the element at `i`, which may be any `int`; negative counts from the end; raises `IndexError` if it's out of range |
| `t[start:stop]` | a new `tuple[T, ...]` of those elements, as a list's slice does (see [list](../list/)) |
| `len(t)` | the number of elements |
| `x in t`, `x not in t` | whether `t` has an element equal to `x` |
| `for x in t:` | each element in turn |
| `let a, b = t` | unpacks it, checking the length when the program runs, as for a list |

`==`, the comparisons and use as a dict key work as for fixed tuples. A fixed tuple, such as a `tuple[int, int]` held in a name, can't be used where a `tuple[int, ...]` is expected, nor the other way round. A literal written in place, as `total((1, 2, 3))`, can.

One with no elements displays as `()`, and one with a single element with a comma after it, as `("x",)`.

```taipan
let names: tuple[str, ...] = ("ada", "alan", "grace")
print(names, len(names), names[1:])
print("alan" in names, names[-1])
for n in names:
    print(n)
print(tuple([1, 2, 3]), tuple(["x"]), tuple(range(0)))
```

```output
("ada", "alan", "grace") 3 ("alan", "grace")
True grace
ada
alan
grace
(1, 2, 3) ("x",) ()
```

Tuples have no methods.
