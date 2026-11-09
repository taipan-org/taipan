---
title: int, float and bool
description: "Numbers and truth values: arithmetic, comparison, overflow, mixing int and float, and converting."
sidebar:
  order: 4
---

An `int` is a whole number, such as `42` or `-7`. It's stored in 64 bits, so it holds values from `-9223372036854775808` to `9223372036854775807`, and arithmetic that goes past either end raises `OverflowError` instead of giving a wrong answer. A `float` is a number with a fractional part, such as `2.5` or `1e9` (a billion), stored as a 64-bit floating-point value, so it holds about 15 to 17 significant digits. A `bool` is `True` or `False`.

Number literals are written in decimal. `_` may go between digits for readability, as in `1_000_000`. A float literal needs a digit before the point (`0.5`, not `.5`), and may have an exponent, as in `2.5e-3`.

None of the three has methods. Everything is done with operators and [built-in functions](../functions/).

## Arithmetic

| Operation | Gives | Example |
|---|---|---|
| `a + b`, `a - b`, `a * b` | the sum, difference or product | `7 * 2` is `14` |
| `a / b` | the quotient, always as a `float`, even for two `int`s | `7 / 2` is `3.5`, `6 / 3` is `2.0` |
| `a // b` | the quotient rounded down to a whole number: an `int` for two `int`s, a `float` if either is a `float` | `7 // 2` is `3`, `-7 // 2` is `-4` |
| `a % b` | the remainder after `//`, with the sign of `b` | `7 % 3` is `1`, `-7 % 3` is `2` |
| `a ** b` | `a` to the power `b` | `2 ** 10` is `1024` |
| `-a` | `a` with its sign changed | |
| `a += b` and the others | `a = a + b`; also `-=`, `*=`, `/=`, `//=`, `%=` and `**=` | |

`//` rounds down, toward negative infinity, not toward zero, so `-7 // 2` is `-4`, and `%` gives the matching remainder, so `a == (a // b) * b + a % b` always holds. `divmod(a, b)` gives both at once.

Dividing by zero with `/`, `//` or `%` raises `ZeroDivisionError`, for `float`s too. `**` with two `int`s gives an `int`, so a negative exponent raises `ValueError`, and is a compile error when it's written as a literal, as in `2 ** -1`: write `2.0 ** -1` for `0.5`. A negative `float` raised to a fractional power, such as `(-8.0) ** 0.5`, raises `ValueError`.

`**` is done before a minus sign on its left, and groups from the right: `-2 ** 2` is `-4`, and `2 ** 3 ** 2` is `2 ** 9`. Otherwise `*`, `/`, `//` and `%` are done before `+` and `-`, and parentheses change the order.

```taipan
print(7 + 2, 7 - 2, 7 * 2)
print(7 / 2, 6 / 3)
print(7 // 2, -7 // 2, 7.5 // 2)
print(7 % 3, -7 % 3, 7 % -3)
print(2 ** 10, 2.0 ** -1, 9 ** 0.5)
print(-2 ** 2, 2 ** 3 ** 2)
```

```output
9 5 14
3.5 2.0
3 -4 3.0
1 2 -2
1024 0.5 3.0
-4 512
```

There are no bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`) on `int`s, and no unary `+`. Each is a compile error.

## Overflow and special values

An `int` result outside the 64-bit range raises `OverflowError`, whether from `+`, `-`, `*`, `**`, `-x`, or `abs` of the smallest `int`. A `float` never raises for being too big: it becomes `inf` (infinity), or `-inf`, and a result with no meaningful value, such as `inf - inf`, is `nan` ("not a number"). A `nan` isn't equal to anything, not even itself.

`float` arithmetic works in binary, so most decimal fractions are stored as the nearest value it can hold: `0.1 + 0.2` is `0.30000000000000004`. Use `round` or a format spec (see [str](../str/#formatting)) to show fewer digits.

```taipan
let biggest = 9223372036854775807
try:
    print(biggest + 1)
except OverflowError as e:
    print("OverflowError:", e)
try:
    print(10 // 0)
except ZeroDivisionError as e:
    print("ZeroDivisionError:", e)
print(1e308 * 10, 0.1 + 0.2)
```

```output
OverflowError: integer overflow
ZeroDivisionError: integer division or modulo by zero
inf 0.30000000000000004
```

## Mixing int and float

An `int` can be used wherever a `float` is expected, and becomes a `float`: an operation with one of each gives a `float`, a list of both is a `list[float]`, and an `int` passed or assigned to a `float` is stored as one. A value shows as its type, so an `int` that became a `float` prints with `.0`. A `float` is never turned into an `int` without `int(…)`, so `let n: int = 2.5` is a compile error, and so is `/=` on an `int` variable, since `/` gives a `float`.

```taipan
let x: float = 1
print(x, 1 + 2.5, [1, 2.5])
print(3 == 3.0, 2 < 2.5, max(3, 2.5))
var total = 0.0
total += 2
print(total)
```

```output
1.0 3.5 [1.0, 2.5]
True True 3.0
2.0
```

## Comparison

| Operation | Gives |
|---|---|
| `a == b`, `a != b` | whether `a` and `b` are equal, or not |
| `a < b`, `a <= b`, `a > b`, `a >= b` | whether `a` is less than, at most, greater than, or at least `b` |
| `a < b < c` | `a < b and b < c`, working out `b` only once; any of the comparisons above can be chained |

Each gives a `bool`. An `int` and a `float` compare by their exact values, so `3 == 3.0` is `True`. Comparing a number with a value of another type, such as `5 == "5"`, is a compile error.

## bool

| Operation | Gives |
|---|---|
| `a and b` | `True` if both are `True`; `b` isn't worked out when `a` is `False` |
| `a or b` | `True` if either is `True`; `b` isn't worked out when `a` is `True` |
| `not a` | `True` if `a` is `False`, and `False` if it's `True` |
| `a == b`, `a != b` | whether both are the same |

`and`, `or` and `not` take only `bool`s, and so do the conditions of `if`, `elif` and `while`: other values aren't treated as true or false, so `if count:` is a compile error. Write the test you mean, as in `if count > 0:`. A `bool` isn't a number either, so `True + 1`, `True < False` and `int(True)` are compile errors.

`not` is done after comparisons, so `not a == b` is `not (a == b)`, and `and` before `or`.

```taipan
let age = 20
let member = False
print(age >= 18 and not member, age < 13 or age > 65)
print(1 < age < 30, age == 20, age != 20)
```

```output
True False
True True False
```

## Converting

| Call | Gives |
|---|---|
| `int(x)` | for a `float`, the whole number part, dropping anything after the point; raises `OverflowError` if it's infinite or too big for an `int`, and `ValueError` for `nan` |
| `int(s)` | for a `str`, an `int?`: the number, or `None` if the text isn't one |
| `float(n)` | for an `int`, the same number as a `float` |
| `float(s)` | for a `str`, a `float?`: the number, or `None` if the text isn't one |
| `round(x)` | for a `float`, the nearest `int`; exactly halfway goes to the even one |
| `round(x, n)` | `x` rounded to `n` digits after the point |
| `str(x)` | the text `print` would write |

`int(s)` accepts an optional sign, digits with `_` allowed between them, and spaces around them. `float(s)` also accepts a fraction, an exponent, `inf`, `infinity` and `nan`. Because the result may be `None`, it has to be checked before use (see [Optionals](../../tour/optionals/)).

`round` rounds the exact value the `float` holds, which is often slightly above or below the decimal written, so `round(2.675, 2)` is `2.67`.

A `float` prints with the fewest digits that read back as the same value, with `.0` when it's whole. Very large and very small ones print with an exponent, `e+16` meaning "times ten to the 16th".

```taipan
print(int(3.9), int(-3.9), float(7))
print(int("  -1_000 ") ?? 0, int("3.5") ?? 0, float("2e-3") ?? 0.0, float("nan") ?? 0.0)
print(round(2.5), round(-1.5), round(2.675, 2))
print(str(1.0), str(1e16), str(0.0001), str(1e-05), str(True))
print(f"{2 / 3:.3f}", f"{1234567:,}", f"{0.25:.0%}")
```

```output
3 -3 7.0
-1000 0 0.002 nan
2 -2 2.67
1.0 1e+16 0.0001 1e-05 True
0.667 1,234,567 25%
```

For every way to format a number in an f-string, see [str](../str/#formatting).
