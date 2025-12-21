# KairosId

**KairosId** is a high-performance C# library for generating unique, time-ordered identifiers. It is designed to be lightweight, efficient, and easy to use in modern .NET applications.

> [!TIP]
> **What's in a name?** The name comes from the ancient Greek word *kairos* (καιρός), which refers to the "opportune moment" or "the right time"—unlike *chronos* (χρόνος), which refers to chronological or sequential time. This library is designed to be exactly that: the opportune identifier for your modern .NET applications.

## Features

- **Time-Ordered**: IDs are sortable by creation time.
- **Unique**: 106-bit identifier space (48-bit timestamp + 58-bit randomness).
- **Compact**: Default Base58 representation is only 18 characters long.
- **High Performance**: Built with `readonly struct` and `UInt128` (C# 12 / .NET 8) to minimize allocations.
- **Flexible Formats**: Supports Base58, Base32 (Crockford), Base16 (Hex), and Base64.
- **No Dependencies**: Clean, self-contained library.

## Installation

This library targets .NET 8 and later. You can reference the project directly or build it as a NuGet package.

## Usage

### Generating IDs

```csharp
using KairosId;

// Generate a new ID (current time)
KairosId id = KairosId.NewKairosId();

// Generate an ID for a specific timestamp
var oldId = KairosId.NewKairosId(DateTimeOffset.UtcNow.AddDays(-1));

Console.WriteLine(id); // Output: Base58 string (e.g., "7Yh9S...")
```

### Parsing IDs

```csharp
// Parse from string (detects format based on length)
KairosId id = KairosId.Parse("your-id-string");

// Explicit parsing for specific formats
KairosId idFromHex = KairosId.ParseHex("...");
KairosId idFromBase32 = KairosId.ParseBase32("...");
KairosId idFromBase64 = KairosId.ParseBase64("...");
```

### Explicit Formatting

While `ToString()` defaults to Base58, you can use explicit methods for better readability:

```csharp
var id = KairosId.NewKairosId();

string b58 = id.ToBase58(); // Default (18 chars)
string b32 = id.ToBase32(); // Crockford (22 chars)
string hex = id.ToHex();    // Hexadecimal (27 chars)
string b64 = id.ToBase64(); // Base64 (24 chars)
```

### Sorting

`KairosId` implements `IComparable<KairosId>`, so you can sort collections of IDs directly. They will be ordered chronologically.

```csharp
var ids = new List<KairosId> { id3, id1, id2 };
ids.Sort(); // Arranges by time-value
```

### Timestamps

You can extract the creation time from any ID:

```csharp
DateTimeOffset timestamp = id.Timestamp;
```

## How It Works

**KairosId** uses a 106-bit structure packing:
- **Timestamp (48 bits)**: High bits. Milliseconds since Jan 1, 2020.
- **Randomness (58 bits)**: Low bits. Fast pseudo-random values (`Random.Shared`).

This structure ensures that IDs generated later will numerically be larger than earlier IDs (monotonicity), while providing sufficient collision resistance for distributed systems.

## Comparisons

Interested in how **KairosId** stacks up against other identifier libraries? Check out our detailed comparisons:

- [**Interface Comparison**](docs/comparison_interface.md): API and usage differences.
- [**Performance Comparison**](docs/comparison_performance.md): Benchmark results and analysis.
- [**Implementation Comparison**](docs/comparison_implementation.md): Technical design and structure.

## Credits

Special thanks to the [**Cysharp/Ulid**](https://github.com/Cysharp/Ulid) library. It served as a vital reference and inspiration for significantly improving the performance of KairosId.
