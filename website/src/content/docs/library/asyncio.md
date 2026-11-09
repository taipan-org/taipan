---
title: asyncio
description: "Running async code: run, gather, sleep, wait_for, timeout, and cancellation."
sidebar:
  order: 11
---

`asyncio` is a module that comes with Taipan, for running async functions: functions declared with `async def`, which can wait for something, such as a timer or a network request, while other code runs. [Async](../../tour/async/) in the tour explains async functions and `await`; this page lists what the module has.

```taipan
import asyncio
```

## Functions

| Call | Gives |
|---|---|
| `asyncio.run(aw)` | runs `aw` to its end and gives its result |
| `asyncio.gather(a, b, …)` | an awaitable that waits for every one of `a`, `b`, … and gives a list of their results, in the order they were given |
| `asyncio.gather(*aws)` | the same, for each awaitable in the list `aws` |
| `asyncio.sleep(seconds)` | an awaitable that finishes after `seconds`, an `int` or `float` |
| `asyncio.wait_for(aw, seconds)` | an awaitable that gives `aw`'s result, or raises `TimeoutError` if `aw` takes longer than `seconds` |
| `asyncio.timeout(seconds)` | a limit for `async with`: the code inside is stopped after `seconds`, and `TimeoutError` raised |
| `asyncio.CancelledError` | the exception that stops an async function that's cancelled |
| `asyncio.TimeoutError` | another name for the built-in `TimeoutError` |

An *awaitable* is a value that will give a result later: `await` waits for it. Calling an async function gives one, as `Awaitable[T]` for a function declared `-> T`.

## Calling an async function starts it

Calling an async function starts it running at once, before anything awaits it. It runs until it has to wait, and the call then gives back an awaitable, while the function goes on later. So two functions called one after the other both run at the same time, and `await` only waits for a result:

```taipan
import asyncio

async def worker(name: str) -> str:
    print(name, "starts")
    await asyncio.sleep(0)
    print(name, "resumes")
    return name

async def main():
    let a = worker("a")
    let b = worker("b")
    print("both started")
    print(await a, await b)

asyncio.run(main())
```

```output
a starts
b starts
both started
a resumes
b resumes
a b
```

This differs from Python, where an async function doesn't start until something awaits it. An awaitable that nothing ever awaits is a compile error, since an error in it would be lost: store it and await it later, or pass it to `gather`.

## run

`asyncio.run(main())` runs async code from a program that isn't itself async, and gives the result. Code after an `await` runs on the program's one thread, so async functions take turns at their `await`s and never run at the same instant.

The main file's top level may also use `await` directly, which runs it the same way, so `asyncio.run` is optional there:

```taipan
import asyncio

async def greet(name: str) -> str:
    await asyncio.sleep(0)
    return "hello " + name

print(await greet("ada"))
```

```output
hello ada
```

`asyncio.run` while async code is already running, in an async function or at the top level after an `await` has run, raises `RuntimeError`, `asyncio.run() cannot be called from a running event loop`.

## gather and sleep

`gather` waits for several awaitables at once and gives a list of their results, in the order they were passed, however long each took. Their results must have one type, as a list's elements do. If one raises an exception, `gather` raises it at once, and the others keep running.

`sleep(seconds)` waits that long without holding up other async code. `sleep(0)` lets everything else that's ready run first.

```taipan
import asyncio

async def square(n: int) -> int:
    await asyncio.sleep(0.01 * n)
    return n * n

async def fails() -> int:
    await asyncio.sleep(0.01)
    raise ValueError("no square")

async def main():
    print(await asyncio.gather(square(3), square(1), square(2)))
    let pending = [square(n) for n in range(4)]
    print(await asyncio.gather(*pending))
    try:
        print(await asyncio.gather(square(5), fails()))
    except ValueError as e:
        print("gather raised:", e)

asyncio.run(main())
```

```output
[9, 1, 4]
[0, 1, 4, 9]
gather raised: no square
```

## Timeouts

`wait_for(aw, seconds)` and `async with timeout(seconds):` put a limit on how long something may take. When the time runs out, they cancel what they're waiting for, as below, and then raise `TimeoutError`. With `None` for `seconds`, there's no limit.

```taipan
import asyncio

async def slow(seconds: float) -> str:
    await asyncio.sleep(seconds)
    return "finished"

async def main():
    print(await asyncio.wait_for(slow(0.01), 1.0))
    try:
        print(await asyncio.wait_for(slow(10), 0.05))
    except TimeoutError:
        print("wait_for timed out")
    try:
        async with asyncio.timeout(0.05):
            print(await slow(10))
    except TimeoutError:
        print("timeout ran out")
    async with asyncio.timeout(None):
        print(await slow(0))

asyncio.run(main())
```

```output
finished
wait_for timed out
timeout ran out
finished
```

`TimeoutError` derives from `OSError` (see [Exceptions](../exceptions/)), so `except OSError` catches it too.

## Cancellation

Cancelling an async function stops it: a `CancelledError` is raised inside it, at the `await` it's waiting at. `wait_for` and `timeout` cancel what's inside them when time runs out, and cancelling a `gather` cancels everything it's waiting for. If the function is waiting for another async function, that one is cancelled instead, and the `CancelledError` reaches the first through its `await`, so the innermost code's `finally` blocks run first.

`CancelledError` derives from `BaseException`, not from `Exception`, so `except Exception` doesn't catch it, and a cancelled function doesn't go on by accident. A function may catch it with `except asyncio.CancelledError`, to clean up and raise it again, or to go on anyway. When the function goes on and finishes, `wait_for` gives its result rather than raising `TimeoutError`:

```taipan
import asyncio

async def tidy() -> str:
    try:
        await asyncio.sleep(10)
        return "finished"
    except asyncio.CancelledError:
        print("tidy: cleaning up")
        raise

async def stubborn() -> str:
    try:
        await asyncio.sleep(10)
    except asyncio.CancelledError:
        print("stubborn: going on")
    return "done anyway"

async def main():
    try:
        print(await asyncio.wait_for(tidy(), 0.01))
    except TimeoutError:
        print("tidy timed out")
    print(await asyncio.wait_for(stubborn(), 0.01))

asyncio.run(main())
```

```output
tidy: cleaning up
tidy timed out
stubborn: going on
done anyway
```

Starting a task without awaiting it (`create_task`), cancelling one directly, and Python's other `asyncio` names, such as `wait`, `as_completed`, queues and locks, aren't in Taipan yet.
