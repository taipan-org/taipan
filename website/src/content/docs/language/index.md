---
title: Overview
description: "Every part of the Taipan language, one page each: what each construct does, its rules, and the errors it gives."
sidebar:
  order: 1
---

Each page here covers one part of the language completely: what each construct does, the rules it follows, the compile errors you get when one is broken, and examples you can run. The [Tour](../tour/basics/) teaches the language from the start, the [Library reference](../library/) covers the built-in types and functions, and the [specification](../reference/specification/) gives the exact rules.

| Page | What's on it |
|---|---|
| [Variables and constants](variables/) | `let`, `var` and `const`, types and inference, assignment, unpacking, scope |
| [Control flow](control-flow/) | `if`, `while`, `for`, `break`, `continue`, comprehensions |
| [match](match/) | matching a value against patterns, as a statement or an expression |
| [Functions](functions/) | `def`, parameters and defaults, keyword arguments, lambdas, functions as values |
| [Classes](classes/) | classes, dataclasses, inheritance, properties, special methods |
| [Enums](enums/) | a value that's one of several variants, each with its own fields |
| [Optionals and narrowing](optionals/) | `T?`, `None`, `is some`, `??`, and how checks narrow a type |
| [Operators](operators/) | every operator, its precedence, indexing and slicing |
| [Strings and f-strings](strings-and-f-strings/) | string literals, escapes, f-strings and format specs |
| [Types](types/) | the built-in types, generics, protocols and type aliases |
| [Modules and imports](modules/) | splitting a program into files, and importing Python and .NET |
| [Errors](errors/) | `try`, `except`, `finally`, `raise` and `assert` |
| [with](with/) | using something for a block and cleaning it up after |
| [async and await](async/) | functions that wait without blocking |
| [Iterators and generators](generators/) | `yield`, and values produced one at a time |
