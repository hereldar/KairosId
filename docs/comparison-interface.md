# Interface Comparison: KairosId vs. Cysharp/Ulid vs. System.Guid

This report compares the public programming interfaces of `KairosId`, the Cysharp `Ulid` library, and the standard .NET `System.Guid` class.

## 1. Creation Methods (Factories)

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Default Generation** | `NewId()` | `NewUlid()` | `NewGuid()` |
| **Timestamp-based** | `NewId(DateTimeOffset)` | `NewUlid(DateTimeOffset)` | `Guid.CreateVersion7()` (since .NET 9) |
| **Random-based** | Internal only | `NewUlid(DateTimeOffset, byte[])` | `new Guid(byte[])` |

**Note**: `KairosId` uses generic naming (`NewId`), while `Ulid` and `Guid` use their specific type names.

## 2. Parsing and Conversion

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Standard Parsing** | `Parse(string, IFormatProvider?)` | `Parse(string)` | `Parse(string)` |
| **Safe Parsing** | `TryParse(string, out KairosId)` | `TryParse(string, out Ulid)` | `TryParse(string, out Guid)` |
| **Span Support** | `TryParse(ReadOnlySpan<char>, ...)` | `Parse(ReadOnlySpan<char>)` | `Parse(ReadOnlySpan<char>)` |
| **Multi-Format** | Base58, Base32, Hex, Base64 | Base32 only (Crockford) | Hex formats (N, D, B, P, X) |
| **Modern Interfaces**| `IParsable<T>` | `ISpanParsable<T>` | `IParsable<T>`, `ISpanParsable<T>` |

**Key Difference**: `KairosId` is built to support multiple string formats (Base58, Base64, etc.) by default. `Ulid` and `Guid` are strictly tied to their specific standards.

## 3. Data Access Properties

| Property | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Timestamp** | `Timestamp` (DateTimeOffset) | `Time` (DateTimeOffset) | Not exposed directly (needs bit shifts) |
| **Raw Value** | `Value` (UInt128) | `ToByteArray()` | `ToByteArray()`, byte access |
| **Randomness** | Not exposed directly | `Random` (ReadOnlySpan<byte>) | Not exposed directly |

**Key Difference**: `KairosId` exposes the raw `UInt128` value, making it easy to work with as a number. `Guid` and `Ulid` focus more on byte arrays and hexadecimal representations.

## 4. String Formatting

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Default Format** | Base58 (18 chars) | Base32 (26 chars) | Hex with hyphens (36 chars) |
| **Custom Formats** | "B58", "B32", "B16", "B64" | Single default | "N", "D", "B", "P", "X" |
| **Sortable Output** | Yes (lexicographical) | Yes (lexicographical) | Only in Version 7 |

## 5. Equality and Comparison

All three implement:
- `IEquatable<T>`
- `IComparable<T>`
- Equality operators (`==`, `!=`)
- Comparison operators (`<`, `>`, `<=`, `>=`)

**Summary**:
`KairosId` offers the most flexibility for different string representations and uses modern C# 12 types. `Cysharp/Ulid` provides a high-performance alternative to `Guid` that is standard-compliant. `System.Guid` is the built-in choice with the best integration into the .NET ecosystem, but it is more rigid in its formatting.
