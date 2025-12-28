# KairosId Internal Design

This document explains the technical details, design decisions, and performance optimizations of the **KairosId** library. It is designed to provide deep knowledge of how the library works under the hood.

## 1. Philosophy and Purpose

**KairosId** is a **105-bit** identifier designed to be:
1.  **Time-Ordered (K-sorted)**: IDs generated sequentially maintain a numerical order.
2.  **Compact**: A representation of only **18 characters** in Base58.
3.  **Efficient**: Implemented as a `readonly struct` that maximizes performance and minimizes memory assignments (zero-allocation).

## 2. Anatomy of the Identifier (The 105 Bits)

Unlike standard GUIDs (128 bits) or ULID (128 bits), KairosId uses exactly **105 bits** packed inside a native C# `UInt128` type.

### Bit Structure (Conceptual Big-Endian)
| Component | Size | Description |
| :--- | :--- | :--- |
| **Reserved** | 23 bits | Zero bits (MSB) to keep the value within the 105-bit range. |
| **Timestamp** | 43 bits | Milliseconds elapsed since the *Custom Epoch*. |
| **Randomness** | 62 bits | Random values to guarantee uniqueness. |

### Key Technical Decisions:
-   **Why 105 bits?** This was specifically chosen to optimize **Base58** representation. $log_{58}(2^{105}) \approx 17.92$, which means a 105-bit value fits perfectly into **18 characters** of Base58 without wasting space or creating ambiguous lengths.
-   **Custom Epoch**: The clock starts on **January 1, 2020 (UTC)**. This allows us to represent dates until the year **2298** using only 43 bits. By not using the Unix Epoch (1970), we save valuable bits for entropy (uniqueness).
-   **Entropy (62 bits)**: With $2^{62}$ combinations per millisecond, the probability of collision in distributed systems is extremely low.

## 3. Implementation and Storage

### Usage of `UInt128`
KairosId uses the native `UInt128` type (introduced in .NET 7/8).
-   **Advantage**: It allows for atomic and extremely fast mathematical operations (bit shifts, comparisons, divisions), taking advantage of processor instructions (ARM64/x64).
-   **Memory**: As a `readonly struct` with a single `UInt128` field, it occupies exactly **16 bytes** in memory, the same as a `Guid`.

#### Why `UInt128` instead of two `ulong` fields?
While some libraries use two 64-bit fields for compatibility with older .NET versions, KairosId uses `UInt128` for several technical reasons:
1.  **Atomicity and Simplicity**: Comparisons for equality and order are performed as a single logical unit. There is no need for complex logic to compare high and low parts manually.
2.  **Bit Manipulation**: Extracting the timestamp (shifting 62 bits) is trivial with `UInt128`. Doing the same across two `ulong` variables requires manual calculations prone to errors.
3.  **JIT/Hardware Optimization**: The .NET 8+ JIT compiler generates highly optimized assembly code for `UInt128`, using specialized instructions (like `LDP/STP` on ARM) that often outperform manual variable management.
4.  **Model Clarity**: Semantically, KairosId *is* a single 105-bit number. Representing it as one unit makes the code much easier to read, maintain, and audit.

## 4. Generation Algorithm

When you call `KairosId.NewKairosId()`:
1.  **Time Calculation**: It gets `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()` and subtracts the `EpochTimestamp` (2020).
2.  **Randomness Generation**: It uses `Random.Shared.NextBytes(8)`.
    -   **Optimization**: `Random.Shared` is preferred over `RandomNumberGenerator` (cryptographic) because it is significantly faster for general business use cases.
3.  **Assembly**:
    ```csharp
    UInt128 combined = (timestampPart << 62) | randomPart;
    ```
    The timestamp is shifted left by 62 positions to occupy the most significant bits, ensuring chronological numerical ordering.

## 5. Encoding and Text Formats

KairosId excels in how it converts binary data to text.

### Base58 (Default Format)
It uses the Bitcoin alphabet (no `0`, `O`, `I`, `l`) to avoid human errors.
-   **Block Optimization**: To avoid slow 128-bit divisions, KairosId uses a two-pass algorithm:
    1.  Divide the value by $58^{10}$ (the largest exponent of 58 that fits in 64 bits).
    2.  Process the remainder (pass 1) and the quotient (pass 2) using faster 64-bit logic.
-   **Unrolled Decoding**: Base58 string parsing is "unrolled," avoiding loops and using precomputed powers of 58.

#### Why is `ulong` division faster?
Although `UInt128` is a native type, most modern processors (x64 and ARM64) only have **hardware instructions** for dividing numbers up to 64 bits. When we ask .NET to divide a `UInt128`, it must emulate that operation via software (long division algorithm), which is expensive. 
By dividing once by $58^{10}$, we reduce the problem to two blocks that the processor can handle directly with its high-speed division circuits.

### Other Formats
-   **Base32 (Crockford)**: 22 characters. Uses pure bit shifts (5 bits per character), making it the fastest format.
-   **Base16 (Hex)**: 27 characters. Includes an implicit prefix to maintain storage size compatibility.

## 6. Performance Optimizations ("Zero-Allocation")

The library follows a strict **zero-allocation** philosophy:
-   **`Span<char>`**: `ReadOnlySpan<char>` is used for parsing and `Span<char>` for formatting, avoiding intermediate string creation.
-   **`string.Create`**: In `ToString()`, this modern API is used to write directly into the final string memory without additional buffered copies.
-   **`stackalloc`**: Stack memory is used for small temporary buffers during generation.

---

[**← Back to README**](../README.md)
