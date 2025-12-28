# KairosId

![GitHub](https://img.shields.io/github/license/hereldar/KairosId)
[![NuGet version (KairosId)](https://img.shields.io/nuget/v/KairosId.svg)](https://www.nuget.org/packages/KairosId)

**KairosId** is a high-performance C# library for generating unique, time-ordered identifiers. It is designed to be lightweight, efficient, and easy to use in modern .NET applications.

> [!TIP]
> **What's in a name?** The name comes from the ancient Greek word *kairos* (καιρός), which refers to the "opportune moment" or "the right time"—unlike *chronos* (χρόνος), which refers to chronological or sequential time. This library is designed to be exactly that: the opportune identifier for your modern .NET applications.

## ✨ Features

- **Time-Ordered**: IDs are sortable by creation time.
- **Compact**: Only **18 characters** long in its default Base58 representation.
- **High Performance**: Built with `readonly struct` and `UInt128` (C# 12 / .NET 8) to minimize allocations.
- **Flexible Formats**: Supports Base58, Base32 (Crockford), and Base16 (Hex).
- **No Dependencies**: Clean, self-contained library.

## 🚀 Quick Start

### Installation

This library targets **.NET 8** and later. You can reference the project directly or build it as a NuGet package.

### Generating IDs

```csharp
using KairosId;

// Generate a new ID (current time)
KairosId id = KairosId.NewKairosId();

// Generate an ID for a specific timestamp
var oldId = KairosId.NewKairosId(DateTimeOffset.UtcNow.AddDays(-1));

Console.WriteLine(id); // Output: Base58 string (e.g., "7Yh9S...")
```

### Parsing and Formatting

```csharp
// Parse from string (detects format based on length)
KairosId id = KairosId.Parse("7Yh9S6K8L3M2N1P4R5");

// Format to string
string b58 = id.ToString(); // Base58 (18 chars)
string b32 = id.ToBase32(); // Crockford (22 chars)
string hex = id.ToHex();    // Hexadecimal (27 chars)
```

### Strongly-Typed IDs

For better domain modeling and type safety, it is highly recommended to wrap `KairosId` in a `readonly record struct`. This prevents accidental assignment of different ID types:

```csharp
public readonly record struct ProductId(KairosId Value)
{
    public static ProductId New() => new(KairosId.NewKairosId());
    public override string ToString() => Value.ToString();
    public static implicit operator KairosId(ProductId id) => id.Value;
}

// Usage
public void ProcessOrder(CustomerId customerId, ProductId productId) { ... }
```

## 📖 Documentation

Whether you are just starting or looking for deep technical details, we have you covered:

### Basics

- **[API Reference](docs/api-reference.md)**: Explore the public contract, implemented interfaces, and a detailed guide to every method.

### Deep Dive

- **[Internal Design](docs/internal-design.md)**: Understand the 105-bit structure, performance optimizations, and the math behind our Base58 encoding.

### Comparisons

- **[Performance](docs/comparison_performance.md)**: Speed and memory benchmark results.
- **[Interface](docs/comparison_interface.md)**: API and usage differences compared to ULID and GUID.
- **[Implementation](docs/comparison_implementation.md)**: Technical design and data layout differences.

## 🛠️ Development

This project uses [CSharpier](https://github.com/belav/csharpier) for code formatting and [Husky.Net](https://github.com/alirezanet/husky.net) for git hooks.

Before pushing changes, the pre-push hook will automatically check the code formatting and run tests. if the code is not formatted, the push will be interrupted.

### Setup Hooks
The hooks are automatically installed during the build/restore process via MSBuild. You normally don't need to run anything manually.

If you ever need to manually reinstall them:

```bash
dotnet tool restore
dotnet husky install
```

### Manual Commands

- **Format code**: `make format`
- **Run tests**: `make tests`
- **Run benchmarks**: `make benchmarks`

## 📜 Credits

Special thanks to the [**Cysharp/Ulid**](https://github.com/Cysharp/Ulid) library. It served as a vital reference and inspiration for significantly improving the performance of KairosId.
