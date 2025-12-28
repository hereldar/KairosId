# Performance Comparison: KairosId vs. Ulid vs. Guid

This report shows how `KairosId` performs compared to `System.Guid` and `Cysharp/Ulid` based on recent benchmarks.

## Hardware and Environment

Benchmarks were executed on the following system:

- **Library**: BenchmarkDotNet v0.15.8
- **OS**: macOS Sequoia 15.7.1 (24G231) [Darwin 24.6.0]
- **CPU**: Apple M4 (2.40GHz, 1 CPU, 10 logical and 10 physical cores)
- **Runtime**: .NET 9.0.3 (Arm64 RyuJIT armv8.0-a)
- **SDK**: .NET SDK 9.0.202

## Benchmark Results

The following table summarizes the speed and memory usage of each library on modern .NET.

| Method           | Mean (Speed) | Ratio | Allocated Memory |
| :--------------- | -----------: | ----: | ---------------: |
| **NewKairosId**  |  **19.94 ns** |  0.11 |          **0 B** |
| NewUlid          |     19.13 ns |  0.10 |              0 B |
| NewGuid          |    183.61 ns |  1.00 |              0 B |
| **KairosIdToString (Base58)** | **81.34 ns** | 0.44 | **64 B** |
| UlidToString (Base32) | 33.98 ns | 0.19 | 80 B |
| GuidToString (Hex) | 195.19 ns | 1.06 | 96 B |
| **ParseKairosId** | **11.09 ns** | 0.06 | **0 B** |
| ParseUlid        |      6.92 ns| 0.04 | 0 B |
| ParseGuid        |      8.46 ns| 0.05 | 0 B |

---

## 1. Creating New IDs (`NewKairosId`)

**KairosId** is as fast as **Ulid** and about **8 times faster** than creating a standard **Guid**.

- **Why is it fast?** It uses specialized code that takes advantage of modern .NET.
- **Randomness:** It uses `Random.Shared` to get high-performance random numbers that are safe for most uses.

## 2. Converting to String (`ToString`)

The speed of `ToString()` depends on the format you choose.

- **Base58 (Default):** Takes about **81 ns**. It is 2.4 times faster than Guid's Hex conversion, while providing a much shorter string (only 18 characters).
- **Base32/Hex:** These are even faster (about **34-37 ns**) because they use simple bit-shifting.
- **Memory:** `KairosId` uses less memory than others when creating strings. It only needs **64 bytes**, while Ulid needs 80 bytes and Guid needs 96 bytes.

| KairosId Format | Mean (Speed) | Allocated |
| :-------------- | -----------: | --------: |
| ToBase58 (Default)|    82.55 ns |      64 B |
| ToBase32        |    34.10 ns |      72 B |
| ToHex           |    36.83 ns |      80 B |

## 3. Parsing IDs

`KairosId` can read (parse) IDs very quickly, taking around **11 ns**. While `Ulid` and `Guid` are slightly faster at parsing, the difference is negligible for most applications.

## Conclusion

`KairosId` offers a great balance between speed and size:
1. **Creation:** Extremely fast, matching industry standards.
2. **Size:** Provides the shortest string representation (Base58).
3. **Efficiency:** Lowest memory usage when converting to text.
4. **Performance:** Optimized Base58 logic makes it faster than even standard Hex conversions of other IDs.

If you need the absolute highest speed for text conversion, you can use `ToBase32()` instead of the default `ToString()`.

