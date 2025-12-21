# KairosID

KairosID is a high-performance C# library for generating unique, time-ordered identifiers. It is designed to be lightweight, efficient, and easy to use in modern .NET applications.

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
using KairosID;

// Generate a new ID
KairosId id = KairosId.NewId();

Console.WriteLine(id); // Output: Base58 string (e.g., "...encoded string...")
```

### Parsing IDs

```csharp
// Parse from string (detects Base58 and Base32 automatically)
KairosId id = KairosId.Parse("your-id-string");

// Parse specific formats
KairosId idFromHex = KairosId.ParseHex("...");
KairosId idFromBase32 = KairosId.ParseBase32("...");
```

### Formatting

You can convert the ID to different string formats using `ToString(format)`.

- **Base58 (Default)**: `"B58"` - 18 characters (e.g., `123...abc`)
- **Base32 (Crockford)**: `"B32"` - 22 characters.
- **Hex (Base16)**: `"B16"` or `"X"` - Standard hexadecimal.
- **Base64**: `"B64"` - Standard Base64.

```csharp
var id = KairosId.NewId();

Console.WriteLine(id.ToString("B58")); // Default
Console.WriteLine(id.ToString("B32"));
Console.WriteLine(id.ToString("B16"));
```

### sorting

Ids implement `IComparable`, so you can sort collections of IDs directly. They will be ordered chronologically by their timestamp.

```csharp
var ids = new List<KairosId> { id3, id1, id2 };
ids.Sort(); // Arranges strictly by time/value
```

### Timestamps

You can extract the creation time from any ID:

```csharp
DateTimeOffset timestamp = id.Timestamp;
```

## How It Works

KairosID uses a 106-bit structure packing:
- **Timestamp (48 bits)**: High bits. Milliseconds since Jan 1, 2020.
- **Randomness (58 bits)**: Low bits. Cryptographically secure random values.

This structure ensures that IDs generated later will numerically be larger than earlier IDs (monotonicity), while providing sufficient collision resistance for distributed systems.
