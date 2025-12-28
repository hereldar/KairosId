# Interface Comparison: KairosId vs. Ulid vs. Guid

This report compares how you use `KairosId`, `Cysharp/Ulid`, and `System.Guid` in your code. It shows the methods and properties available in each library.

## 1. Creating New IDs

How to create a new ID in your application:

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **New ID** | `KairosId.NewKairosId()` | `Ulid.NewUlid()` | `Guid.NewGuid()` |
| **From Date** | `NewKairosId(DateTimeOffset)` | `NewUlid(DateTimeOffset)`| `Guid.CreateVersion7(DateTimeOffset)` |
| **Custom Random** | Internal only | `NewUlid(DateTimeOffset, byte[])` | `new Guid(byte[])` |

---

## 2. Converting and Reading Text

`KairosId` is flexible and supports many text formats by default.

| Feature | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Default Text** | Base58 (18 chars) | Base32 (26 chars) | Hex (36 chars) |
| **Other Formats** | Base32, Hex | Base32 only | Various Hex formats |
| **Reading Text** | `Parse(string)` | `Parse(string)` | `Parse(string)` |
| **Safe Reading** | `TryParse(string, out id)`| `TryParse(string, out id)`| `TryParse(string, out id)`|

**Key Difference:** `KairosId` makes it easy to switch between different text formats (like Base32 or Hex) using simple methods. `Ulid` and `Guid` are mostly stuck with their one standard format.

---

## 3. Getting Data from the ID

What information can you get out of an existing ID?

| Property | KairosId | Cysharp/Ulid | System.Guid |
| :--- | :--- | :--- | :--- |
| **Timestamp** | `Timestamp` | `Time` | Not easy to get |
| **Raw Value** | `Value` (UInt128) | `ToByteArray()` | `ToByteArray()` |

**Key Difference:** `KairosId` gives you direct access to the `UInt128` value, which is very useful for mathematical operations or comparisons.

---

## 4. Comparing IDs

All three libraries work exactly as you would expect when comparing:
- You can use `==` and `!=` to see if they are the same.
- You can use `<`, `>`, `<=`, and `>=` to sort them.
- They all work perfectly in Lists and Dictionaries.

## Summary

- **KairosId** has a simple, modern interface that is easy for developers to use. It's the best choice if you need flexible text formatting.
- **Ulid** is a great standardized alternative to Guid.
- **Guid** is the built-in option that everyone knows, but it lacks some modern features like easy access to the creation time.
