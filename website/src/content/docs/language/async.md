---
title: async and await
description: "Async functions and await: when they start, the event loop, async generators, async for, async with, and the checks for awaitables nobody waits for."
sidebar:
  order: 15
---

An *async function* is one that can pause while it waits for something slow, such as a timer, a network request or another async function, and let other code run in the meantime. It's declared with `async def`. Calling it gives an *awaitable*, a value that will hold the function's result once it finishes, and `await` waits for that result.

All of a program's async code runs on one thread. Only one piece of Taipan code runs at a time, and the program switches from one async function to another only at an `await`.

Async functions are defined in [§3.14](../../reference/specification/#314-async-functions-m33), and async generators, `async for`, `async with` and the event loop in [§3.20](../../reference/specification/#320-generators-and-async-ii-m42). The functions for running async code, such as `gather`, `sleep` and `wait_for`, are listed in [asyncio](../../library/asyncio/).

## `async def` and `Awaitable[T]`

`async def f(…) -> T` declares an async function. Its declared return type is what `await` gives, so calling it gives an `Awaitable[T]`, and `await f(…)` gives a `T`. Without a return type, the call gives `Awaitable[None]`. Methods can be async too.

```taipan
import asyncio

async def double(n: int) -> int:
    await asyncio.sleep(0)
    return n * 2

let pending = double(21)
print(pending)
print(await pending)
```

```output
<awaitable>
42
```

An awaitable displays as `<awaitable>`, and two awaitables are equal only when they're the same one. As a function value, `double` has type `Callable[[int], Awaitable[int]]`.

## Calling starts the function

Calling an async function starts it at once. Its body runs until its first `await` of something that hasn't finished yet, and the call returns then; the rest of the body runs later, when what it's waiting for finishes. So two calls made one after the other are both under way before either is awaited, and `await` only waits for a result that's already being worked on.

```taipan
import asyncio

async def worker(name: str) -> str:
    print(name, "starts")
    await asyncio.sleep(0)
    print(name, "finishes")
    return name.upper()

let a = worker("a")
let b = worker("b")
print("both started")
print(await a, await b)
```

```output
a starts
b starts
both started
a finishes
b finishes
A B
```

This differs from Python on purpose, matching how .NET tasks start: in Python, calling an async function runs none of it until it's awaited ([§3.14](../../reference/specification/#314-async-functions-m33)).

## `await`

`await e` waits for the awaitable `e` and gives its value. If the async function ended with an exception, `await` raises that exception. Awaiting something that isn't an awaitable is `TP0200`:

```taipan
let n = 5
print(await n)
```

```error
main.tpn:2:13: error TP0200: cannot await int
```

`await` binds as tightly as a unary minus, so `await f(x).y` awaits `f(x).y`. To take a field of the result, write `(await f(x)).y`.

### Where `await` may appear

- In the body of an `async def`.
- At the top level of the main file. A file whose top level has an `await` runs as an async program: the event loop runs until the top level finishes.

Anywhere else is `TP0215`: in an ordinary `def`, including a `def` nested inside an `async def`, in a lambda, in an ordinary generator, and in a `const`.

```taipan
import asyncio

def tick():
    await asyncio.sleep(0)

tick()
```

```error
main.tpn:4:5: error TP0215: 'await' outside an async function
```

## The event loop and `asyncio.run`

The *event loop* is what runs async code: when an `await` waits for something, the loop runs whatever else is ready, and resumes the waiting function, on the program's main thread, when what it awaited finishes. Functions resume in the order the things they waited for finished. Code that never awaits runs straight through.

There are two ways to run the loop:

- An `await` at the top level of the main file, as in the examples above.
- `asyncio.run(aw)`, an ordinary function that runs the loop until `aw` finishes and gives its result.

```taipan
import asyncio

async def main() -> int:
    let results = await asyncio.gather(asyncio.sleep(0), asyncio.sleep(0))
    print(len(results), "done")
    return 0

print(asyncio.run(main()))
```

```output
2 done
0
```

Calling `asyncio.run` while the loop is already running, such as in an async function that has resumed from an `await`, raises `RuntimeError`:

```taipan
import asyncio

async def inner() -> int:
    return 1

async def outer():
    await asyncio.sleep(0)
    print(asyncio.run(inner()))

asyncio.run(outer())
```

```error
Traceback (most recent call last):
  File "main.tpn", line 10, in <module>
    asyncio.run(outer())
  File "main.tpn", line 8, in outer
    print(asyncio.run(inner()))
RuntimeError: asyncio.run() cannot be called from a running event loop
```

## Every awaitable is awaited

An exception from an async function only reaches the program through `await`, so an awaitable that nothing awaits would lose its errors. The compiler checks for this:

- Calling an async function as a statement on its own, throwing the awaitable away, is error `TP0235`.
- A `let` or `var` holding an awaitable that's never used is warning `TP0307`.

```taipan
import asyncio

async def save(name: str):
    await asyncio.sleep(0)
    print("saved", name)

save("a")
```

```error
main.tpn:7:1: error TP0235: this awaitable is never awaited
  help: write 'await …'
```

```taipan
import asyncio

async def save(name: str):
    await asyncio.sleep(0)
    print("saved", name)

let pending = save("a")
await asyncio.sleep(0.01)
```

```output
saved a
```

```error
main.tpn:7:5: warning TP0307: 'pending' is never awaited
  help: await it, or pass it to 'asyncio.gather'
```

An awaitable can still escape the compiler's view, for instance by being stored in a list that's never awaited. So when the program ends, each awaitable that failed with nothing awaiting it prints `Task exception was never retrieved` and its traceback to stderr. The exit code doesn't change, and a cancelled one isn't reported.

```taipan
import asyncio

async def fail(n: int):
    raise ValueError(f"task {n} failed")

let tasks = [fail(1)]
print(len(tasks), "task started")
```

This prints `1 task started`, and then, when the program ends, this to stderr, with exit code 0:

```text
Task exception was never retrieved
Traceback (most recent call last):
  File "main.tpn", line 4, in fail
    raise ValueError(f"task {n} failed")
ValueError: task 1 failed
```

## Async generators

An `async def` whose body has a `yield` is an *async generator*: a sequence of values, each of which may take some waiting to produce. It's declared to return `AsyncIterator[T]`, `AsyncIterable[T]`, or `AsyncGenerator[T, S]` to receive values with `asend`. Any other return type is `TP0200`.

Unlike an async function, calling an async generator runs none of its body. Its body runs as values are asked for, as an ordinary [generator](../generators/)'s does.

- It can't `return` a value (`TP0200`), and `yield from` can't be used in one (`TP0215`).
- Its methods are `__anext__()` and `asend(v)`, which give an `Awaitable[T]` of the next value and raise `StopAsyncIteration` when it's finished, and `athrow(e)` and `aclose()`, which behave as an ordinary generator's `throw` and `close` do.
- The built-in `anext(it)` is `it.__anext__()`, and `anext(it, default)` gives `default` instead of raising at the end.
- It displays as `<async_generator object f>`.

## `async for`

`async for x in e:` goes over an async iterable, awaiting each value in turn. Otherwise it works as `for` does, with unpacking, `break` and `continue`. `async for` and `async with` may appear only where `await` may.

```taipan
import asyncio

async def ticks(name: str, n: int) -> AsyncIterator[str]:
    for i in range(n):
        await asyncio.sleep(0)
        yield f"{name}{i}"

async def consume(name: str) -> int:
    var count = 0
    async for t in ticks(name, 2):
        print(t)
        count += 1
    return count

let gen = ticks("x", 1)
print(gen)
print(await anext(gen), await anext(gen, "end"))
print(await asyncio.gather(consume("a"), consume("b")))
```

```output
<async_generator object ticks>
x0 end
a0
b0
a1
b1
[2, 2]
```

A class is an async iterable when it has `__aiter__(self) -> AsyncIterator[T]` and an `async def __anext__(self) -> T` that raises `StopAsyncIteration` at the end. Using `async for` over an ordinary list or iterator is `TP0200`, with help to use `for`:

```taipan
async def main():
    async for x in [1, 2]:
        print(x)

await main()
```

```error
main.tpn:2:20: error TP0200: cannot iterate over list[int] with 'async for'
  help: use 'for'
```

Async comprehensions, with `async for` inside the brackets, are not supported yet.

## `async with`

`async with e as v:` is [`with`](../with/) for a manager whose setup and cleanup are async: it awaits `e.__aenter__()` before the block and `e.__aexit__(…)` after it. A class is an async context manager when it has both of these, with the same parameters as `__enter__` and `__exit__`:

- `async def __aenter__(self) -> T`
- `async def __aexit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> bool` (or `-> None`)

Both must be async, and a class must define both or neither (`TP0225`). An ordinary manager in `async with`, or an async one in `with`, is `TP0200`, with help naming the other form. `asyncio.timeout(seconds)` is an async context manager that cancels its block after that many seconds.

```taipan
import asyncio
from types import TracebackType

class Connection:
    async def __aenter__(self) -> Connection:
        await asyncio.sleep(0)
        print("connected")
        return self

    async def __aexit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> None:
        await asyncio.sleep(0)
        print("closed")

async def main():
    async with Connection() as c:
        print("using", c)
    with Connection() as c:
        print("using", c)

await main()
```

```error
main.tpn:17:10: error TP0200: Connection is not a context manager
  help: use 'async with'
```

With `async with` for both, it runs:

```taipan
import asyncio
from types import TracebackType

class Connection:
    async def __aenter__(self) -> Connection:
        await asyncio.sleep(0)
        print("connected")
        return self

    async def __aexit__(self, exc_type: type[BaseException]?, exc: BaseException?, tb: TracebackType?) -> None:
        await asyncio.sleep(0)
        print("closed")

async def main():
    async with Connection():
        print("using it")
    async with asyncio.timeout(1.0):
        await asyncio.sleep(0)
        print("in time")

await main()
```

```output
connected
using it
closed
in time
```

## Cancellation

A running async function can be cancelled by `asyncio.wait_for` or `asyncio.timeout` when its time runs out, or by cancelling a `gather` that's waiting for it. Cancelling raises `asyncio.CancelledError` at the `await` where the function is paused, so its `finally` blocks run; if it's waiting for another async function, that one is cancelled first. `CancelledError` derives from `BaseException`, so `except Exception` doesn't catch it. A function may catch it and carry on. See [asyncio](../../library/asyncio/#cancellation) for examples.

```taipan
import asyncio

async def slow() -> str:
    try:
        await asyncio.sleep(10)
        return "finished"
    finally:
        print("cleaning up")

try:
    print(await asyncio.wait_for(slow(), 0.05))
except TimeoutError:
    print("timed out")
```

```output
cleaning up
timed out
```
