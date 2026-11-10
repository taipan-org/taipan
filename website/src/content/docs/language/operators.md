---
title: Operators
description: "Every operator and its precedence: arithmetic, comparison and chaining, and, or and not, in, is, ??, indexing and slicing, and the operators Taipan doesn't have."
sidebar:
  order: 9
---

An operator combines values into a new value: `a + b`, `not ready`, `xs[0]`. Each operator accepts only certain types, which the compiler checks, so `1 + "a"` is a compile error rather than a failure when the program runs.

Operators are defined in [§5.3](../../reference/specification/#53-operators), indexing in [§5.4](../../reference/specification/#54-indexing), and their precedence by the grammar in [§2](../../reference/specification/#2-grammar-ebnf).

## Precedence

When an expression has several operators, those higher in this table are applied first, so `1 + 2 * 3` is `1 + (2 * 3)`, which is `7`. Operators on the same row are applied from left to right, except where the table says otherwise. Parentheses override the order: `(1 + 2) * 3` is `9`.

| Precedence | Operators | Notes |
|---|---|---|
| 1 (lowest) | `lambda` | a function value, described on [Functions](../functions/) |
| 2 | `??` | groups from the right: `a ?? b ?? c` is `a ?? (b ?? c)` |
| 3 | `or` | |
| 4 | `and` | |
| 5 | `not` | |
| 6 | `==` `!=` `<` `<=` `>` `>=` `in` `not in` `is` `is not` | comparisons, which can be [chained](#chained-comparisons) |
| 7 | `\|` | set union |
| 8 | `^` | set symmetric difference |
| 9 | `&` | set intersection |
| 10 | `+` `-` | |
| 11 | `*` `/` `//` `%` | |
| 12 | `-x`, `await x` | a minus sign in front of one value |
| 13 | `**` | groups from the right: `2 ** 3 ** 2` is `2 ** 9` |
| 14 | `x as T` | a conversion |
| 15 (highest) | `f(…)`, `x.name`, `x[…]` | a call, a field or method, an index or slice |

`**` binds more tightly than a minus sign on its left, so `-2 ** 2` is `-(2 ** 2)`, which is `-4`. On its right, a minus belongs to the exponent: `2 ** -1.0` is `0.5`.

```taipan
let x = 2
print(1 + 2 * 3, (1 + 2) * 3, -x ** 2, (-x) ** 2, 2 ** 3 ** 2)
print(x > 1 and x < 5 or x == 10, not x == 3)
```

```output
7 9 -4 4 512
True True
```

## Arithmetic

| Operator | Operands | Gives |
|---|---|---|
| `a + b`, `a - b`, `a * b` | two `int`s | an `int` |
| `a + b`, `a - b`, `a * b` | numbers, at least one a `float` | a `float` |
| `a / b` | any two numbers | always a `float`: `7 / 2` is `3.5`, and `6 / 3` is `2.0` |
| `a // b` | numbers | `a / b` rounded down to a whole number: an `int` for two `int`s, a `float` otherwise |
| `a % b` | numbers | the remainder after `//`, with the sign of `b` |
| `a ** b` | numbers | `a` to the power `b` |
| `-a` | a number | `a` with its sign changed |
| `a + b` | two `str`s, or two lists of the same type | a new string or list, `a` followed by `b` |
| `s * n`, `n * s` | a `str` or a list, and an `int` | a new string or list holding `n` copies of `s`; empty when `n` is `0` or less |

An `int` and a `float` may be mixed in any arithmetic: the `int` is turned into a `float` first. Nothing else mixes. A `bool` isn't a number, and a string can't be added to a number, so convert explicitly with `str(n)`, `int(s)` or `float(s)` (see [Numbers](../../library/numbers/)).

```taipan
print(7 + 2, 7 - 2, 7 * 2, 7 / 2, 7 // 2, 7 % 2, 7 ** 2)
print(1 + 2.5, 6 / 3, 2 ** 0.5)
print("ab" + "cd", "ab" * 3, [1] + [2], [0] * 3)
```

```output
9 5 14 3.5 3 1 49
3.5 2.0 1.4142135623730951
abcd ababab [1, 2] [0, 0, 0]
```

```taipan
print("total: " + 3)
```

```error
main.tpn:1:7: error TP0200: operator '+' cannot be applied to str and int
```

### Rounding down, and remainders

`//` always rounds *down*, toward negative infinity, not toward zero, and `%` gives a remainder with the same sign as the right operand (or zero). Together, `(a // b) * b + a % b` is always `a`.

```taipan
print(-7 // 2, -7 % 2, 7 % -2, -7.5 // 2, -7.5 % 2)
```

```output
-4 1 -1 -4.0 0.5
```

### Powers

`**` with two `int`s gives an `int`, and `0 ** 0` is `1`. A negative exponent on an `int` base would give a fraction, so it's an error: a compile error when the exponent is written as a literal, and a `ValueError` when it's computed. Use a `float` base, as in `2.0 ** -1`.

```taipan
print(2 ** -1)
```

```error
main.tpn:1:12: error TP0200: an int can't be raised to a negative power
  help: use a float base, as in 2.0 ** -1
```

With a `float`, `0.0 ** -1` raises `ZeroDivisionError`, and a negative base with a fractional exponent, such as `(-8.0) ** 0.5`, raises `ValueError`.

### Overflow and division by zero

An `int` is 64 bits: from `-9223372036854775808` to `9223372036854775807`. A result outside that range raises `OverflowError`, rather than wrapping around to a wrong number. An integer literal outside the range is a compile error.

Dividing by zero, with `/`, `//` or `%`, raises `ZeroDivisionError`, for `float`s as well as `int`s.

```taipan
let big = 9223372036854775807
print(big + 1)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 2, in <module>
    print(big + 1)
OverflowError: integer overflow
```

```taipan
print(1 / 0)
```

```error
Traceback (most recent call last):
  File "main.tpn", line 1, in <module>
    print(1 / 0)
ZeroDivisionError: division by zero
```

### Repeating a list

`xs * n` repeats the elements themselves, not copies of them. When the elements are lists, dicts or sets, every copy is the same one, so a change to one shows in all of them. The compiler warns when you write that, and suggests a comprehension, which builds a separate one each time:

```taipan
var grid = [[0, 0]] * 2
grid[0][0] = 5
print(grid)
let separate = [[0, 0] for _ in range(2)]
print(separate)
```

```output
[[5, 0], [5, 0]]
[[0, 0], [0, 0]]
```

```error
main.tpn:1:12: warning TP0304: every copy shares the same list
  help: build each one separately, with a comprehension: [… for _ in range(…)]
```

## Comparison

| Operator | Means |
|---|---|
| `a == b`, `a != b` | equal, not equal |
| `a < b`, `a <= b`, `a > b`, `a >= b` | less than, at most, greater than, at least |

Each gives a `bool`. The two sides must have related types: comparing an `int` with a `str`, for example, is a compile error rather than always `False`.

**Equality** compares values, not where they're stored. Lists, dicts, sets and tuples are equal when their contents are equal (dicts and sets in any order), enum values when they're the same variant with equal fields, and dataclass objects when their fields are equal. An object of a plain class is equal only to itself, unless its class defines `__eq__` (see [Classes](../classes/)). An `int` and a `float` compare by their exact values, so `1 == 1.0` is `True`. An optional may be compared with `==` to a value or to `None`.

**Ordering** (`<` and the others) works on numbers, on strings, which compare character by character by their Unicode code points (so `"Z" < "a"`), and on tuples of those, which compare element by element from the left. A class can be ordered by defining `__lt__` and the other comparison methods. Lists, dicts and `bool`s can't be ordered.

```taipan
print(1 == 1.0, [1, 2] == [1, 2], {"a": 1} == {"a": 1}, 0.1 + 0.2 == 0.3)
print("apple" < "banana", "Z" < "a", (1, "b") < (1, "c"), 2 >= 2.0)
```

```output
True True True False
True True True True
```

```taipan
print(1 == "1")
```

```error
main.tpn:1:7: error TP0200: cannot compare int and str
```

### Chained comparisons

Comparisons can be chained: `a < b < c` means `a < b and b < c`, except that `b` is computed only once. Each value is computed from left to right, and the chain stops at the first comparison that's `False`. Any mix of `==`, `!=`, `<`, `<=`, `>` and `>=` may be chained.

```taipan
let x = 5
print(0 < x < 10, 0 < x > 7, 1 <= x <= 5 == 5)
```

```output
True False True
```

A chain can't include `in`, `not in`, `is` or `is not`, since chains like `x in xs in ys` are easy to misread. Split it with `and`:

```taipan
let xs = [1, 2]
print(1 in xs == True)
```

```error
main.tpn:2:15: error TP0212: a chained comparison can't include 'in' or 'is'
  help: split it with 'and'
```

## `and`, `or` and `not`

| Operator | Gives `True` when |
|---|---|
| `a and b` | both are `True` |
| `a or b` | either is `True` |
| `not a` | `a` is `False` |

The operands must be `bool`s, and the result is a `bool`. There's no treating other values as true or false (see [Conditions](../control-flow/#conditions-are-bool)), so `not name` on a string is an error: write `name == ""`.

`and` and `or` *short-circuit*: they compute the right side only when it's needed. `a and b` doesn't compute `b` when `a` is `False`, and `a or b` doesn't when `a` is `True`. So the left side can guard the right one: `i < len(xs) and xs[i] > 0` never reads past the end of `xs`.

```taipan
def check(label: str, result: bool) -> bool:
    print(f"checking {label}")
    return result

print(check("a", False) and check("b", True))
print(check("c", True) or check("d", True))
```

```output
checking a
False
checking c
True
```

```taipan
let count = 3
print(not count)
```

```error
main.tpn:2:11: error TP0205: condition must be bool, found int
```

## `in` and `not in`

`x in c` is `True` when the collection `c` contains `x`, and `x not in c` is its opposite.

| `c` | `x` | `True` when |
|---|---|---|
| `list[T]` | a `T` | an element equals `x` |
| `set[T]` | a `T` | `x` is one of the elements |
| `dict[K, V]` | a `K` | `x` is a key (values aren't searched) |
| `str` | a `str` | `x` appears in `c`, anywhere |
| `range` | an `int` | the range includes `x` |

```taipan
let ages = {"ana": 31}
print(2 in [1, 2], 3 not in {1, 2}, "ana" in ages, 31 in ages.values(), "ell" in "hello", 5 in range(0, 10, 5))
```

```output
True True True True True True
```

```taipan
print(1 in "123")
```

```error
main.tpn:1:7: error TP0200: operator 'in' cannot be applied to int and str
```

## `is` and `is not`

`is` has two uses:
- **`x is None` and `x is not None`** test whether an optional holds a value. `x` must have an optional type (`T?`); for any other type it's an error, since the answer would always be the same. The test also *narrows* `x`: after `if x is not None:`, `x` can be used as a `T`. See [Optionals](../optionals/).
- **`a is b` and `a is not b`** test *identity*: whether two names refer to the very same object, not just equal ones. It applies to objects of classes, lists, dicts and sets. Numbers, strings, `bool`s, tuples and enum values have no identity, so `is` on them is an error whose help says to use `==`.

```taipan
let maybe: int? = None
print(maybe is None, maybe is not None)
let a = [1, 2]
let b = a
let c = [1, 2]
print(a is b, a is c, a == c)
```

```output
True False
True False True
```

```taipan
let n = 3
print(n is 3)
```

```error
main.tpn:2:7: error TP0200: 'is' compares identity, which int doesn't have
  help: use '=='
```

```taipan
let n = 3
print(n is None)
```

```error
main.tpn:2:7: error TP0200: 'is None' needs an optional, found int
```

`x is some v` is a third form, which can only be a whole condition of an `if`, `elif` or `while`. It's described on [Optionals](../optionals/).

## `??`

`a ?? b` gives the value of the optional `a` when it isn't `None`, and `b` when it is. `b` is computed only when it's needed. When `b` is a `T`, the result is a `T`, no longer optional, so `??` is the usual way to supply a default:

```taipan
let ages = {"ana": 31}
let age = ages.get("zoe") ?? 0
print(age, ages.get("ana") ?? 0)
let first: str? = None
let second: str? = None
print(first ?? second ?? "nobody")
```

```output
0 31
nobody
```

The left side must be optional:

```taipan
let n = 3
print(n ?? 0)
```

```error
main.tpn:2:7: error TP0200: '??' needs an optional on the left, found int
```

`??` has the lowest precedence of all the operators except `lambda`, so `d.get(k) ?? 0 + 1` means `d.get(k) ?? (0 + 1)`. Arithmetic directly to the right of `??`, without parentheses, gives warning `TP0305`, which shows both readings. Add parentheses to say which you mean:

```taipan
let counts = {"a": 1}
print(counts.get("a") ?? 0 + 1)
print((counts.get("a") ?? 0) + 1)
```

```output
1
2
```

```error
main.tpn:2:23: warning TP0305: '??' binds more loosely than '+', so this is 'counts.get("a") ?? (0 + 1)'
  help: write '(counts.get("a") ?? 0) + 1' if you meant that, or add the parentheses to keep this
```

## Set operators

On two sets of the same type, `|`, `&`, `-` and `^` make a new set, and `<=`, `<`, `>=` and `>` compare them. These operators are defined in [§3.15](../../reference/specification/#315-sets-m35), and described in the [set reference](../../library/set/).

| Operator | Gives |
|---|---|
| `a \| b` | every element of either |
| `a & b` | the elements in both |
| `a - b` | the elements of `a` that aren't in `b` |
| `a ^ b` | the elements in exactly one of them |
| `a <= b`, `a < b` | whether every element of `a` is in `b` (and, for `<`, `b` has more) |
| `a >= b`, `a > b` | the same, the other way round |

```taipan
let a = {1, 2, 3}
let b = {3, 4}
print(a | b, a & b, a - b, a ^ b, {1, 2} <= a, a < a)
```

```output
{1, 2, 3, 4} {3} {1, 2} {1, 2, 4} True False
```

## Indexing

`x[i]` reads one element:

| `x` | `i` | Gives |
|---|---|---|
| `list[T]` | an `int` | the element at position `i`, counting from `0` |
| `str` | an `int` | the character at position `i`, as a one-character `str` |
| `dict[K, V]` | a `K` | the value for key `i` |
| a tuple | an integer literal | that element, with its own type |

A negative index counts from the end, so `xs[-1]` is the last element. An index out of range raises `IndexError`, and a key that isn't in the dict raises `KeyError`. To get `None` for a missing key instead, use `d.get(key)`, or `d.get(key, default)`.

```taipan
let xs = [10, 20, 30]
let word = "taipan"
let ages = {"ana": 31}
let pair = (1, "one")
print(xs[0], xs[-1], word[1], ages["ana"], pair[1])
```

```output
10 30 a 31 one
```

```taipan
let xs = [10, 20, 30]
print(xs[3])
```

```error
Traceback (most recent call last):
  File "main.tpn", line 2, in <module>
    print(xs[3])
IndexError: list index out of range
```

A tuple's index must be a literal number, so the compiler knows the element's type. A variable index is an error:

```taipan
let pair = (1, "one")
let i = 0
print(pair[i])
```

```error
main.tpn:3:12: error TP0200: tuple index must be an integer literal
```

Assigning to an element, `xs[i] = v` or `d[k] = v`, changes the list or dict. Strings and tuples can't change, so assigning to one of their elements is an error. Assignment is described on [Variables and constants](../variables/#assignment).

## Slicing

`x[start:stop]` gives a new list or string holding the elements from `start` up to, but not including, `stop`. `x[start:stop:step]` takes every `step`-th element. Slicing works on lists and strings.

| Slice | Gives |
|---|---|
| `xs[a:b]` | elements `a` to `b - 1` |
| `xs[a:]` | from `a` to the end |
| `xs[:b]` | from the start up to `b - 1` |
| `xs[:]` | a copy of the whole list |
| `xs[::2]` | every second element |
| `xs[::-1]` | every element, in reverse |

The rules:
- A missing `start` or `stop` means the start or end. With a negative `step`, which walks backwards, they mean the end and the start.
- A negative `start` or `stop` counts from the end, as an index does.
- A `start` or `stop` beyond the ends is clipped to them, so slicing never raises `IndexError`.
- A `step` of `0` raises `ValueError`.
- Each part is an `int`, or an `int?` whose `None` means the part is missing.
- The result is always new, so changing it doesn't change the original.

```taipan
let xs = [10, 20, 30, 40, 50]
print(xs[1:3], xs[:2], xs[3:], xs[-2:], xs[::2], xs[::-1], xs[1:100])
let word = "taipan"
print(word[1:4], word[::-1])
```

```output
[20, 30] [10, 20] [40, 50] [40, 50] [10, 30, 50] [50, 40, 30, 20, 10] [20, 30, 40, 50]
aip napiat
```

Tuples and dicts can't be sliced:

```taipan
let pair = (1, 2)
print(pair[0:1])
```

```error
main.tpn:2:7: error TP0200: cannot slice tuple[int, int]
```

A slice of a list can be assigned, replacing those elements with the elements of another list, which may have a different length. `del xs[a:b]` removes them. See [`del`](../variables/#del).

```taipan
var xs = [1, 2, 3, 4, 5]
xs[1:3] = [7, 7, 7]
print(xs)
xs[::2] = [0, 0, 0]
print(xs)
```

```output
[1, 7, 7, 7, 4, 5]
[0, 7, 0, 7, 0, 5]
```

With a `step`, the new list must have exactly as many elements as the slice, or it raises `ValueError`.

## Operators Taipan doesn't have

Some operators found in other languages don't exist in Taipan. Each is a compile error:

| Written | Instead |
|---|---|
| `a if c else b` | a [`match` expression](../match/#match-expressions) on `c`, or an `if` statement |
| `&`, `\|`, `^` on `int`s, `~`, `<<`, `>>` | none yet: operators on the bits of an `int` are planned for later |
| `&=`, `\|=`, `^=`, `<<=`, `>>=` | none yet, as above; on sets, write `a = a \| b` |
| `+x` | `x` |
| `a @ b` | a function |
| `n := value` | a separate `let` or assignment before the expression |
| `x is 3`, `s is "a"` | `x == 3`, `s == "a"` |
| `*=` on a list | `xs = xs * n` |

```taipan
let n = 3
let size = "big" if n > 10 else "small"
```

```error
main.tpn:2:18: error TP0100: Taipan has no conditional expression
  help: use a 'match' expression or an 'if' statement
```

```taipan
print(6 & 3)
```

```error
main.tpn:1:7: error TP0200: operator '&' cannot be applied to int and int
```

```taipan
print(1 << 3)
```

```error
main.tpn:1:9: error TP0100: Taipan has no '<<' operator
  help: shift and bitwise operators on int aren't supported yet
```

```taipan
print(~6)
```

```error
main.tpn:1:7: error TP0004: invalid character '~'
  help: Taipan has no bitwise operators on int yet
```

```taipan
print(+3)
```

```error
main.tpn:1:7: error TP0100: Taipan has no unary '+'
  help: leave it out
```

```taipan
let xs = [1, 2]
print(n := len(xs))
```

```error
main.tpn:2:9: error TP0100: expected ')', found ':'
```
