# Implementation Comparison: KairosId vs. Ulid vs. Guid

This report explains the technical differences between `KairosId`, `Cysharp/Ulid`, and `System.Guid`. It helps you understand how each library works under the hood.

## 1. How Data is Stored

All three libraries use 16 bytes of memory, but they handle the data differently inside.

| Aspect | KairosId               | Cysharp/Ulid | System.Guid |
| :--- |:-----------------------| :--- | :--- |
| **Tech Used** | Uses native `UInt128`  | Uses two `long` fields | Uses four integers |
| **Simplicity** | Very simple code       | Complex for speed | Built into Windows/Core |
| **Compatibility**| Needs .NET 8 or higher | Works on old .NET | Works everywhere |

**Why it matters:** `KairosId` uses modern .NET features (`UInt128`) to keep the code clean and easy to maintain. `Ulid` and `Guid` use older methods to remain compatible with older versions of .NET.

## 2. What's Inside the ID?

Each ID is made of a **Timestamp** (when it was created) and **Randomness** (to make it unique).

| Component | KairosId | Cysharp/Ulid | System.Guid (v7) |
| :--- | :--- | :--- | :--- |
| **Timestamp** | 43 bits (ms) | 48 bits (ms) | 48 bits (ms) |
| **Date Starts At**| **Jan 1, 2020** | Jan 1, 1970 | Jan 1, 1970 |
| **Max Uniqueness**| 62 random bits | 80 random bits | 74 random bits |
| **Empty Space** | 23 bits | 0 bits | 6 bits |

**The KairosId Advantage:** By starting our "clock" at the year 2020 instead of 1970, we can fit the ID into a much smaller string (18 characters) without losing accuracy.

## 3. How IDs are Generated

- **KairosId:** Fast and safe. It uses `Random.Shared` which is highly optimized for performance in modern apps.
- **Cysharp/Ulid:** Extremely fast. It focus on high-throughput scenarios and includes extra features to ensure IDs generated at the exact same millisecond are still in order.
- **System.Guid:** The official standard. .NET 9 introduced **Version 7**, which is much better for databases than the old random Guids.

## Summary

- **Choose KairosId** if you want the **shortest possible IDs** and you are using a modern version of .NET (8+). It is perfect for clean, modern code.
- **Choose Ulid** if you need to follow the official ULID standard strictly or need to support very old systems.
- **Choose System.Guid** if you want to stay with the built-in .NET tools and don't mind the longer 36-character strings.

---

[**← Back to README**](../README.md)
