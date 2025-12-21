# Implementation Comparison: KairosId vs. Cysharp/Ulid vs. System.Guid

This report compares the technical design and internal implementation of `KairosId`, the Cysharp `Ulid` library, and the built-in `System.Guid`.

## 1. Data Structure and Storage

| Aspect | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Total Size** | 106 effective bits | 128 bits | 128 bits |
| **Internal Storage**| `UInt128 _value` | Two `long` fields or bytes | Four integers/bytes |
| **Memory Usage** | 16 bytes | 16 bytes | 16 bytes |

**Key Difference**: `KairosId` uses the native `UInt128` type introduced in .NET 7. This simplifies bit manipulation and comparisons. `System.Guid` and `Ulid` use more traditional structures to maintain compatibility with older .NET versions.

## 2. Bit Composition

| Component | KairosId | Cysharp/Ulid | System.Guid (v7) |
| :--- | :--- | :--- | :--- |
| **Timestamp** | 48 bits (ms) | 48 bits (ms) | 48 bits (ms) |
| **Epoch** | **Jan 1, 2020** | Jan 1, 1970 (Unix) | Jan 1, 1970 (Unix) |
| **Randomness** | 58 bits | 80 bits | 74 bits |
| **Empty Bits** | 22 bits | 0 bits | 6 bits (version/variant) |

**Key Difference**: By using a modern epoch (2020), `KairosId` fits comfortably in a shorter string (18 chars Base58). `Guid` and `Ulid` follow international standards that require tracking time back to 1970.

## 3. ID Generation

### Randomness
- **KairosId**: Uses `RandomNumberGenerator.Fill` for every ID. It is cryptographically secure but involves a system call.
- **Cysharp/Ulid**: Optimized for high speed. Includes support for **monotonicity** (incrementing the random part if multiple IDs are generated in the same millisecond).
- **System.Guid**: .NET 9's version 7 implementation is highly optimized and follows the RFC 9562 standard for time-based UUIDs.

### Performance
- **KairosId**: Uses `string.Create` and `Span<char>` to avoid extra memory allocations. Performance is excellent on modern .NET but lacks some of the extreme low-level optimizations (like SIMD) found in `Ulid`.
- **Cysharp/Ulid**: Heavily optimized with vectorization (SIMD) and `unsafe` code to be the fastest ULID runner possible across all .NET versions.
- **System.Guid**: Built into the runtime, making it highly optimized by the JIT compiler, but strictly limited to its defined formats.

## 4. Platform Support

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **C# Version** | C# 12+ required | Wide support (older C#) | Native to .NET |
| **Frameworks** | .NET 7/8+ only | .NET Core, Framework, Unity | All .NET platforms |

## Design Summary

- **KairosId** is a "modern-first" library. It sacrifices backward compatibility to gain code simplicity and a very compact string representation (18 characters). It is perfect for new projects that want clean code and efficient database keys.
- **Cysharp/Ulid** is the industry standard for ULID in .NET. It is extremely fast and works everywhere. It is the best choice if you need to follow the official ULID specification strictly.
- **System.Guid** is the safest choice for general purpose use. With the addition of **Version 7** in .NET 9, it now offers time-based sorting natively, though its string representation remains the longest (36 characters).
