---
title: dict
description: "Dictionaries: making them, operators, every method, and counting, grouping and looping."
sidebar:
  order: 6
---

A `dict[K, V]` maps keys of type `K` to values of type `V`, as `dict[str, int]` maps words to counts. Each key appears once, and the keys keep the order they were first added in.

```taipan
let ages = {"ada": 36, "alan": 41}
var counts: dict[str, int] = {}
```

The type is inferred from a literal with something in it, so `ages` is a `dict[str, int]`. An empty `{}` needs its type written. All the keys share one type, and so do all the values: for records with fields of different types, use a [dataclass](../../tour/classes/), and for JSON, `dict[str, py]`.

A dict declared with `let` can still be changed: `let` only means the name can't be given a different dict.

Keys must be `int`, `str` or `bool` values, tuples of those, or instances of plain classes. Lists and dicts can't be keys, since they can change.

## Operators

| Operation | Gives |
|---|---|
| `d[k]` | the value for `k`; raises `KeyError` if there's none |
| `d[k] = v` | sets the value for `k`, adding the key if it's new |
| `del d[k]` | removes `k`; raises `KeyError` if it's not there |
| `k in d`, `k not in d` | whether `d` has the key `k` |
| `len(d)` | the number of keys |
| `for k in d:` | each key in turn, in order |
| `d == other` | whether both have the same keys and values, in any order |

Adding or removing keys while looping over a dict raises `RuntimeError`. Changing the value of an existing key is fine.

## Methods

| Method | Gives |
|---|---|
| `d.get(k)` | the value for `k` as an optional, `None` if there's none |
| `d.get(k, default)` | the value for `k`, or `default` if there's none |
| `d.keys()`, `d.values()` | a new list of the keys, or of the values, in order |
| `d.items()` | a new `list[tuple[K, V]]` of the keys and values, in order |
| `d.pop(k)` | removes `k` and gives its value; raises `KeyError` if it's not there |
| `d.pop(k, default)` | the same, but gives `default` if `k` isn't there |
| `d.remove(k)` | removes `k` if it's there, and gives whether it was; Taipan's own, not Python's |
| `d.setdefault(k, default)` | the value for `k`, first adding `k` with `default` if it isn't there |
| `d.update(other)` | adds or replaces every key and value from `other` |
| `d.copy()` | a new dict with the same keys and values |
| `d.clear()` | removes every key |

`get(k)` with no default gives an optional (`V?`), which has to be checked before it's used. Give a default, or use `is some` (see [Optionals](../../tour/optionals/)):

```taipan
let ages = {"ada": 36}
print(ages.get("ada", 0) + 1)
if ages.get("bob") is some age:
    print(age)
else:
    print("no bob")
```

```output
37
no bob
```

## Counting and grouping

`get` with a default counts things, and `setdefault` groups them:

```taipan
var counts: dict[str, int] = {}
for word in ["the", "taipan", "the"]:
    counts[word] = counts.get(word, 0) + 1
print(counts)

var by_letter: dict[str, list[str]] = {}
for word in ["ant", "bee", "asp"]:
    by_letter.setdefault(word[0], []).append(word)
print(by_letter)
```

```output
{"the": 2, "taipan": 1}
{"a": ["ant", "asp"], "b": ["bee"]}
```

## Looping

`items()` gives each key with its value, and `sorted` puts them in another order. Its `key` says what to sort by: here the count, from highest, then the word:

```taipan
let counts = {"in": 4, "the": 10, "and": 4}
for word, n in counts.items():
    print(word, n)
for word, n in sorted(counts.items(), key=lambda p: (-p[1], p[0])):
    print(word, n)
```

```output
in 4
the 10
and 4
the 10
and 4
in 4
```

## Making dicts

| Form | Gives |
|---|---|
| `{"a": 1, "b": 2}` | a dict of those keys and values |
| `{k: f(k) for k in xs}` | a dict comprehension: one key and value for each `k` |
| `dict(zip(keys, values))` | a dict from two lists side by side |
| `dict(a=1, b=2)` | a `dict[str, …]` from names and values |
| `dict(d)` | a copy of `d` |

```taipan
print({n: n * n for n in range(4)})
print(dict(zip(["x", "y"], [1, 2])))
```

```output
{0: 0, 1: 1, 2: 4, 3: 9}
{"x": 1, "y": 2}
```
