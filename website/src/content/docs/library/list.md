---
title: list
description: "Lists: indexing and slicing, operators, every method, sorting, comprehensions, and changing a list in a loop."
sidebar:
  order: 5
---

A `list[T]` is an ordered collection of values of type `T` that can grow, shrink and change, as `list[int]` holds whole numbers. Positions count from 0, and the same value may appear more than once.

```taipan
let primes = [2, 3, 5]
var names: list[str] = []
names.append("ada")
print(primes, names, [1, 2.5])
```

```output
[2, 3, 5] ["ada"] [1.0, 2.5]
```

The type is inferred from a literal with something in it, so `primes` is a `list[int]`. An empty `[]` needs its type written, and so do function parameters and return types. All the elements share one type: an `int` and a `float` mix to `float`, as `[1, 2.5]` shows, and objects of classes with a common base mix to that base.

A list declared with `let` can still be changed: `let` only means the name can't be given a different list. Assigning a list to another name doesn't copy it, so both names see the same changes. Use `xs.copy()` or `xs[:]` for a separate list.

Lists are .NET `List<T>` objects, so .NET's own `List<T>` methods and properties, such as `xs.Count` and `xs.BinarySearch(x)`, can be called on them too (see [Using .NET](../../guides/dotnet/)).

## Operators

| Operation | Gives |
|---|---|
| `xs[i]` | the element at `i`, counting from 0; a negative `i` counts from the end, so `xs[-1]` is the last; raises `IndexError` if `i` is out of range |
| `xs[i] = x` | replaces the element at `i` |
| `xs[start:stop]` | a new list of the elements from `start` up to, not including, `stop` |
| `xs[start:stop:step]` | the same, taking every `step`th element; a negative `step` goes backwards, so `xs[::-1]` is the list reversed |
| `xs[start:stop] = ys` | replaces those elements with the elements of `ys`, which may be more or fewer |
| `del xs[i]` | removes the element at `i`; raises `IndexError` if `i` is out of range |
| `del xs[start:stop]` | removes those elements |
| `a + b` | a new list of `a`'s elements followed by `b`'s |
| `xs += ys` | adds `ys`'s elements to the end of `xs`, changing it |
| `xs * n` | a new list of `xs`'s elements repeated `n` times; `[]` if `n` is 0 or less |
| `x in xs`, `x not in xs` | whether `xs` has an element equal to `x` |
| `len(xs)` | the number of elements |
| `for x in xs:` | each element in turn, in order |
| `a == b`, `a != b` | whether both have equal elements in the same order |

Any of the three parts of a slice may be left out: `start` defaults to the beginning, `stop` to the end, and `step` to 1. Negative bounds count from the end, and bounds past either end are moved to it, so a slice never raises, except for a `step` of 0, which raises `ValueError`. A slice is always a new list. When `step` is given, a slice assignment must have exactly as many elements as the slice, or it raises `ValueError`.

Lists can't be compared with `<`, `>`, `<=` or `>=`, and `xs *= n` isn't allowed: write `xs = xs * n`. Repeating a list that holds a list, as in `[[0] * 3] * 2`, gives copies that all share that one inner list, which is warning `TP0304`; build each with a comprehension instead, as `[[0] * 3 for _ in range(2)]`.

```taipan
let xs = [10, 20, 30, 40, 50]
print(xs[0], xs[-1])
print(xs[1:3], xs[:2], xs[3:], xs[-2:])
print(xs[::2], xs[::-1])
print([1, 2] + [3], [0] * 3)
print(20 in xs, 25 not in xs, len(xs))
```

```output
10 50
[20, 30] [10, 20] [40, 50] [40, 50]
[10, 30, 50] [50, 40, 30, 20, 10]
[1, 2, 3] [0, 0, 0]
True True 5
```

Assigning to an element or a slice, and `del`, change the list itself:

```taipan
var xs = [1, 2, 3, 4, 5]
xs[0] = 9
print(xs)
xs[1:3] = [7, 7, 7]
print(xs)
del xs[0]
print(xs)
del xs[::2]
print(xs)
```

```output
[9, 2, 3, 4, 5]
[9, 7, 7, 7, 4, 5]
[7, 7, 7, 4, 5]
[7, 4]
```

## Methods

| Method | Gives |
|---|---|
| `xs.append(x)` | adds `x` to the end |
| `xs.extend(ys)` | adds every value of `ys`, which can be a list, a set, a range or anything else a `for` loop can go over, to the end |
| `xs.insert(i, x)` | puts `x` before position `i`; a negative `i` counts from the end, and an `i` past either end puts `x` at that end |
| `xs.pop()` | removes the last element and gives it; raises `IndexError` if the list is empty |
| `xs.pop(i)` | removes the element at `i` and gives it; a negative `i` counts from the end; raises `IndexError` if `i` is out of range |
| `xs.remove(x)` | removes the first element equal to `x`; raises `ValueError` if there's none |
| `xs.clear()` | removes every element |
| `xs.index(x)` | the position of the first element equal to `x`; raises `ValueError` if there's none |
| `xs.count(x)` | how many elements are equal to `x` |
| `xs.contains(x)` | whether `xs` has an element equal to `x`, as `x in xs` does; Taipan's own, not Python's |
| `xs.copy()` | a new list with the same elements |
| `xs.reverse()` | reverses the order of the elements, in place |
| `xs.sort()` | sorts the elements in place, from smallest (see [Sorting](#sorting)) |
| `xs.sort(key=f, reverse=True)` | sorts in place by what `f` gives for each element, and from largest with `reverse=True` |

The methods that change a list give nothing back, except `pop`, so `let ys = xs.sort()` is an error. "Equal" means equal by `==`, so `[[1], [2]].index([2])` is 1.

```taipan
var xs = [3, 1, 2]
xs.append(4)
xs.insert(0, 0)
xs.extend([5, 6])
print(xs)
print(xs.pop(), xs.pop(0))
print(xs)
xs.remove(1)
print(xs, xs.index(2), xs.count(2))
print(xs.contains(4), 4 in xs)
```

```output
[0, 3, 1, 2, 4, 5, 6]
6 0
[3, 1, 2, 4, 5]
[3, 2, 4, 5] 1 1
True True
```

## Sorting

`xs.sort()` puts the list itself in order and gives nothing. `sorted(xs)` leaves `xs` as it was and gives a new sorted list; it takes any collection, not only a list (see [Built-in functions](../functions/)). Both take the same two optional arguments, which must be given by name:

- `key=f` sorts by what the function `f` gives for each element, rather than by the elements themselves. `f` is called once for each element.
- `reverse=True` sorts from largest to smallest.

The elements, or the keys, must be numbers, strings, tuples of those, or objects of a class that defines `<` (with `__lt__`). Anything else, such as a list of lists, is an error that asks for a `key`. Numbers sort by value and strings character by character, so `"Zoe"` comes before `"ada"`, since capital letters come before small ones.

The sort is stable: elements with equal keys keep the order they had, with `reverse=True` too. Here `"pear"` and `"kiwi"` both have 4 letters, so they stay in their original order:

```taipan
var words = ["pear", "fig", "apple", "kiwi"]
print(sorted(words))
print(words)
words.sort(key=len)
print(words)
words.sort(key=len, reverse=True)
print(words)
```

```output
["apple", "fig", "kiwi", "pear"]
["pear", "fig", "apple", "kiwi"]
["fig", "pear", "kiwi", "apple"]
["apple", "pear", "kiwi", "fig"]
```

To sort by several things, give a key that makes a tuple, as `key=lambda p: (p.age, p.name)`: tuples compare by their first elements, then their second, and so on (see [tuple](../tuple/)).

## List comprehensions

A list comprehension builds a new list from another collection in one expression. `[e for x in xs]` gives `e` for each `x` in `xs`, and an `if` at the end keeps only the values for which it's `True`. Several `for`s go through every combination, the last changing fastest:

```taipan
print([x * x for x in range(6) if x % 2 == 0])
print([(c, n) for c in "ab" for n in [1, 2]])
```

```output
[0, 4, 16]
[("a", 1), ("a", 2), ("b", 1), ("b", 2)]
```

The names a comprehension's `for` introduces exist only inside it.

## Changing a list in a loop

Adding to or removing from a list while a `for` loop goes over it raises `RuntimeError`, `list changed size during iteration`, at the loop's next step. This is one of the few places Taipan differs from Python, which carries on and silently skips or repeats elements.

```taipan
var xs = [1, 2, 3]
for x in xs:
    if x == 2:
        xs.append(4)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 2, in <module>
    for x in xs:
RuntimeError: list changed size during iteration
```

Replacing elements with `xs[i] = x`, or reordering them with `sort` or `reverse`, doesn't change the size, so it's allowed. A `break` straight after the change also ends the loop without an error. To remove elements, build a new list instead:

```taipan
let xs = [1, 2, 3, 4]
var kept: list[int] = []
for x in xs:
    if x != 2:
        kept.append(x)
print(kept)
print([x for x in xs if x != 2])
```

```output
[1, 3, 4]
[1, 3, 4]
```

## Lists of a base class

A `list[Dog]` can't be passed where a `list[Animal]` is expected, even when `Dog` derives from `Animal`. Code holding it as a `list[Animal]` could append a `Cat` to it, and then the `list[Dog]` would hold a cat. To make a list that may hold any animal, write its type when you make it:

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

Without the `: list[Animal]`, `dogs` would be a `list[Dog]`, and `count(dogs)` would be an error: `expected list[Animal], found list[Dog]`.

## Making lists

| Form | Gives |
|---|---|
| `[1, 2, 3]` | a list of those values |
| `[e for x in xs]` | a list comprehension (above) |
| `list(xs)` | a new list of the values of any collection, in order: `list("abc")` is `["a", "b", "c"]` |
| `list(range(n))` | the numbers from 0 up to, not including, `n` |
| `[x] * n` | `n` copies of `x` |

Many functions and methods also give lists, such as a string's `split` (see [str](../str/)) and a dict's `keys` (see [dict](../dict/)).
