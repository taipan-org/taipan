---
title: set
description: "Sets: making them, union, intersection and the other operators, and every method."
sidebar:
  order: 7
---

A `set[T]` is a collection of distinct values of type `T`: adding a value that's already there does nothing. Sets answer "is this value in here?" quickly, however many values they hold, and combine with each other through union, intersection and difference.

Sets have no positions, so `s[0]` is an error. They do keep their values in the order they were first added, for loops and printing, but that order isn't something to depend on: if the order matters, sort the values with `sorted(s)`, which gives a list.

The values must be ones that could be dict keys: `int`, `str` or `bool` values, tuples of those, or instances of plain classes. Lists, dicts and other sets can't be in a set, since they can change.

## Making sets

| Form | Gives |
|---|---|
| `{"a", "b"}` | a set of those values, each once |
| `set()` | an empty set; its type has to be written, as in `var s: set[str] = set()` |
| `set(xs)` | a set of the values of any collection, such as a list, a string or a range |
| `{e for x in xs}` | a set comprehension: `e` for each `x` in `xs`, with an `if` at the end to keep only some |

`{}` is an empty [dict](../dict/), not an empty set, so an empty set is always written `set()`.

```taipan
let seen = {"b", "a", "b"}
var tags: set[str] = set()
print(seen, tags, len(seen))
print(set([3, 1, 3, 2]), set("hello"))
print({n % 3 for n in range(10)})
```

```output
{"b", "a"} set() 2
{3, 1, 2} {"h", "e", "l", "o"}
{0, 1, 2}
```

A set declared with `let` can still be changed: `let` only means the name can't be given a different set.

## Operators

| Operation | Gives |
|---|---|
| `a \| b` | a new set of the values in `a`, `b` or both (the union) |
| `a & b` | a new set of the values in both `a` and `b` (the intersection) |
| `a - b` | a new set of the values in `a` but not in `b` (the difference) |
| `a ^ b` | a new set of the values in exactly one of `a` and `b` (the symmetric difference) |
| `x in s`, `x not in s` | whether `s` holds `x` |
| `a <= b` | whether every value in `a` is also in `b` (`a` is a subset of `b`) |
| `a < b` | whether `a <= b` and `b` holds something more |
| `a >= b`, `a > b` | the same, the other way round (`a` is a superset of `b`) |
| `a == b`, `a != b` | whether both hold the same values, in any order |
| `len(s)` | the number of values |
| `for x in s:` | each value in turn |

Both sides of each operator must be sets of the same type: to combine a set with a list, use the methods below, which take any collection. The new sets hold `a`'s values first, then `b`'s.

```taipan
let a = {1, 2, 3}
let b = {3, 4}
print(a | b, a & b, a - b, a ^ b)
print(2 in a, 5 not in a)
print({1, 2} <= a, {1, 2} < a, a < a, a >= {3})
print({1, 2} == {2, 1})
```

```output
{1, 2, 3, 4} {3} {1, 2} {1, 2, 4}
True True
True True False True
True
```

Adding values to a set, or removing them, while a `for` loop goes over it raises `RuntimeError`, `Set changed size during iteration`. `sorted`, `list`, `min`, `max`, `sum`, `any` and `all` take sets as they take lists (see [Built-in functions](../functions/)).

## Changing a set

| Method | Gives |
|---|---|
| `s.add(x)` | adds `x`, if it isn't there already |
| `s.remove(x)` | removes `x`; raises `KeyError` if it isn't there |
| `s.discard(x)` | removes `x` if it's there, and does nothing if it isn't |
| `s.pop()` | removes a value and gives it: the first one added of those still there; raises `KeyError` if the set is empty |
| `s.update(xs)` | adds every value of `xs`, which can be any collection |
| `s.clear()` | removes every value |

```taipan
var s = {"a", "b"}
s.add("c")
s.add("a")
print(s)
s.remove("b")
s.discard("z")
print(s)
print(s.pop(), s)
s.update(["x", "y"])
print(s)
```

```output
{"a", "b", "c"}
{"a", "c"}
a {"c"}
{"c", "x", "y"}
```

## Combining and comparing

These are the operators' counterparts, but each takes any collection of the same type, not only a set: a list, a range, or the keys of a [dict](../dict/). Each takes one argument.

| Method | Gives |
|---|---|
| `s.union(xs)` | a new set of the values in `s`, `xs` or both |
| `s.intersection(xs)` | a new set of the values in both `s` and `xs` |
| `s.difference(xs)` | a new set of the values in `s` but not in `xs` |
| `s.symmetric_difference(xs)` | a new set of the values in exactly one of `s` and `xs` |
| `s.issubset(xs)` | whether every value in `s` is in `xs` |
| `s.issuperset(xs)` | whether every value in `xs` is in `s` |
| `s.isdisjoint(xs)` | whether `s` and `xs` have no value in common |
| `s.copy()` | a new set with the same values |

```taipan
let u = {1, 2, 3}
print(u.union([3, 4]), u.intersection(range(2, 10)))
print(u.difference([1]), u.symmetric_difference([3, 4]))
print(u.issubset([1, 2, 3, 4]), u.issuperset([1]), u.isdisjoint([7, 8]))
```

```output
{1, 2, 3, 4} {2, 3}
{2, 3} {1, 2, 4}
True True True
```

The operators that would change a set in place, `-=`, `|=`, `&=` and `^=`, are compile errors, because they'd make a new set where Python changes the shared one. The error says what to write: `s = s & other`, which makes a new set, or `s.update(xs)` and the other methods above, which change the set itself.

```taipan
var seen = {1, 2}
seen |= {3}
```

```error
main.tpn:2:1: error TP0200: '|=' isn't supported on a set
  help: write 's = s | t', which makes a new set
```

## Removing duplicates

A set remembers what has been seen, and `sorted(set(xs))` gives the distinct values of `xs` in order:

```taipan
let words = ["the", "cat", "the", "hat", "cat"]
var seen: set[str] = set()
var first: list[str] = []
for w in words:
    if w not in seen:
        seen.add(w)
        first.append(w)
print(first, len(seen))
print(sorted(set(words)))
```

```output
["the", "cat", "hat"] 3
["cat", "hat", "the"]
```
