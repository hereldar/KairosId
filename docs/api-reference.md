# KairosId API Reference

This document explains the standard .NET interfaces implemented by **KairosId**, why they are important, and provide a detailed description of every public method available.

## 1. Implemented Interfaces and Their Purpose

KairosId is designed to integrate perfectly with the .NET ecosystem by implementing several key interfaces.

### `IEquatable<KairosId>`
- **Purpose**: Allows for efficient and type-safe equality comparison (avoiding boxing).
- **Importance**: Essential for using KairosId as a key in dictionaries (`Dictionary<KairosId, T>`) or in collections like `HashSet`.

### `IComparable<KairosId>`
- **Purpose**: Defines a natural ordering for objects.
- **Importance**: Allows for using the `.Sort()` method in lists and performing relational comparisons (`<`, `>`). Since KairosId is *K-sorted*, this interface guarantees chronological sorting.

### `ISpanParsable<KairosId>`
- **Purpose**: Standardizes how to convert text into a KairosId instance.
- **Importance**: Enables .NET tools (CLI libraries, serializers, Minimal APIs) to automatically convert strings into KairosId objects.

### `ISpanFormattable`
- **Purpose**: Provides a standard way to convert the object to text efficiently.
- **Importance**: Allows KairosId to work optimally with string interpolation (`$"{id}"`) and high-performance buffers, avoiding unnecessary memory allocations.

## 2. Usage Examples (Direct and Indirect)

While you often won't call these interface methods manually, .NET uses them "under the hood" to provide extreme performance.

### `ISpanFormattable` Example (Efficient Formatting)
When you use string interpolation or `string.Format`, .NET checks if the object is `ISpanFormattable` to write directly to the destination without creating intermediate strings.

```csharp
var id = KairosId.NewKairosId();

// Indirect usage via interpolation (Automatically optimized)
string message = $"The ID is: {id:B58}"; 

// Direct usage with high-performance buffers (Zero-allocation)
Span<char> buffer = stackalloc char[18];
if (id.TryFormat(buffer, out int written, "B58", null))
{
    // The buffer now contains the ID in Base58 without creating a string object
}
```

### `ISpanParsable` Example (High-Performance Parsing)
This interface allows for analyzing text fragments (`ReadOnlySpan`) that may be part of a much larger buffer (like a large JSON or log file), avoiding the cost of copying sub-strings.

```csharp
ReadOnlySpan<char> logLine = "2023-12-28 INFO User:7Yh9S6K8L3M2N1P4R5 logged in";
ReadOnlySpan<char> idPart = logLine.Slice(21, 18); // "7Yh9S6K8L3M2N1P4R5"

// Direct usage of ISpanParsable (No new string created for parsing)
if (KairosId.TryParse(idPart, null, out var id))
{
    Console.WriteLine($"ID recovered: {id.Timestamp}");
}

// Generic usage (Where type T is resolved at runtime)
public T ParseIdentifier<T>(string input) where T : IParsable<T>
{
    return T.Parse(input, null);
}
var myId = ParseIdentifier<KairosId>("7Yh9S6K8L3M2N1P4R5");
```

## 3. Detailed Method Guide

### Creating IDs
- **`NewKairosId()`**: Generates a new ID with the current timestamp (UTC) and 62 bits of randomness. This is the main entry point.
- **`NewKairosId(DateTimeOffset timestamp)`**: Allows generating an ID with a specific date. Useful for migrations or recreating historical states.

### Parsing and Conversion
- **`Parse(string s)`**: Converts a string into a KairosId object. Automatically detects the format (Base58, Base32, or Hex) based on the length.
- **`TryParse(string s, out KairosId result)`**: Safe version that returns a boolean instead of throwing an exception if the format is incorrect. Recommended for user input.
- **`ParseBase58 / ParseBase32 / ParseHex`**: Specific methods for when you already know the string format beforehand. These are more direct and clear.

### Comparison and Equality Logic
- **`Equals(KairosId other)`**: Compares the 128-bit value of both instances.
- **`CompareTo(KairosId other)`**: Returns a value indicating if this ID is less than, equal to, or greater than the other. The comparison is based primarily on the timestamp and then on randomness.
- **Operators (`==`, `!=`, `<`, `>`, `<=`, `>=`)**: Overloaded so you can compare KairosIds as easily as numbers or dates.

### Data Output (Formatting)
- **`ToString()`**: By default, returns the Base58 representation (18 characters).
- **`ToString(string format)`**: Allows specifying formats like `"B58"`, `"B32"`, or `"B16"` (Hexadecimal).
- **`TryFormat(...)`**: The high-performance method that writes directly to a buffer (`Span<char>`). Used internally by .NET to optimize memory.

### Helper Methods
- **`ToByteArray()`**: Exports the internal value as a 16-byte array (Big Endian). Useful for binary storage or network protocols.
- **`ToBase58() / ToBase32() / ToHex()`**: Convenience methods that are equivalent to calling `ToString` with the corresponding format, but more explicit in code.

---

[**← Back to README**](../README.md)
