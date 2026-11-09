---
title: Overview
description: "The functions, types and modules built into Taipan, and where each is documented."
sidebar:
  order: 1
---

Every Taipan program has these built in. Each page lists everything its type or module offers: what each operator, function and method takes, what it gives, and what it raises, with examples you can run. The [Tour](../tour/basics/) teaches the language from the start, and the [specification](../reference/specification/#6-built-ins-v01) gives the exact rules.

| Page | What's on it |
|---|---|
| [Built-in functions](functions/) | `print`, `input`, `len`, `range`, `sorted`, `min`, `max`, `zip`, `enumerate`, converting between types, and the rest |
| [str](str/) | text: joining, splitting, searching, changing case, padding, and format specs |
| [int, float and bool](numbers/) | numbers and truth values: arithmetic, comparing, converting |
| [list](list/) | ordered collections that can change: adding, removing, sorting, slicing |
| [dict](dict/) | keys mapped to values: looking up, counting, grouping |
| [set](set/) | collections without duplicates: membership, union, intersection |
| [tuple](tuple/) | fixed groups of values, such as a pair, and unpacking them |
| [Files](files/) | opening, reading and writing text files, and the errors they raise |
| [sys and os](sys-and-os/) | command-line arguments, exit codes, standard streams, environment variables |
| [asyncio](asyncio/) | running async functions, waiting for several at once, timeouts |
| [Exceptions](exceptions/) | the built-in exception classes, what raises each, and catching them |

`sys`, `os` and `asyncio` need an import, as in `import sys`. Everything else is there without one.

Some types belong to a part of the language rather than to the library, and are described with it: optionals (`T?`) with [Optionals](../tour/optionals/), `Callable` with [functions](../tour/functions/), `Iterator` and `Generator` with [iterators and generators](../tour/iterators-and-generators/), `py` with [Calling Python](../guides/python/), and protocols with [generics and protocols](../tour/generics-and-protocols/).
