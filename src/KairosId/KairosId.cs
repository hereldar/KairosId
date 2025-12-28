using System.Diagnostics.CodeAnalysis;
using KairosId.Formats;

namespace KairosId;

/// <summary>
/// Represents a 105-bit identifier composed of a 43-bit timestamp and 62-bit randomness.
/// The identifier is designed to be time-ordered (k-sorted).
/// Default string representation is Base58 (18 characters).
/// </summary>
public readonly struct KairosId
    : IEquatable<KairosId>,
        IComparable<KairosId>,
        IParsable<KairosId>,
        ISpanParsable<KairosId>,
        ISpanFormattable
{
    private const int TimestampBits = 43;
    private const int RandomBits = 62;
    private const long EpochTimestamp = 1577836800000; // Jan 1 2020 UTC in ms

    // The internal 128-bit storage.
    // Layout (Big Endian conceptual view): [00...00 (23 bits)] [Timestamp (43 bits)] [Random (62 bits)]
    private readonly UInt128 _value;

    /// <summary>
    /// Gets the raw 128-bit value of the identifier.
    /// </summary>
    public UInt128 Value => _value;

    /// <summary>
    /// A read-only instance of the KairosId struct whose value is all zeros.
    /// </summary>
    public static KairosId Empty => default;

    /// <summary>
    /// Gets the timestamp component of the identifier.
    /// </summary>
    public DateTimeOffset Timestamp
    {
        get
        {
            // Shift down 62 bits to get timestamp
            long timestampOffset = (long)(_value >> RandomBits);
            return DateTimeOffset.FromUnixTimeMilliseconds(
                timestampOffset + EpochTimestamp
            );
        }
    }

    private KairosId(UInt128 value)
    {
        _value = value;
    }

    /// <summary>
    /// Generates a new unique KairosId.
    /// </summary>
    /// <returns>A new KairosId.</returns>
    public static KairosId NewKairosId()
    {
        return NewKairosId(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Generates a new KairosId for a specific timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp to use.</param>
    /// <returns>A new KairosId.</returns>
    public static KairosId NewKairosId(DateTimeOffset timestamp)
    {
        long msSinceEpoch = timestamp.ToUnixTimeMilliseconds() - EpochTimestamp;

        if (msSinceEpoch < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timestamp),
                "Timestamp must be after Jan 1 2020."
            );
        }

        // 48 bits check
        if (msSinceEpoch >= (1L << TimestampBits))
        {
            throw new ArgumentOutOfRangeException(
                nameof(timestamp),
                "Timestamp too far in the future."
            );
        }

        // Generate 62 bits of randomness
        Span<byte> randomBytes = stackalloc byte[8];

        // Use Random.Shared for performance (userspace PRNG)
        // This is significantly faster than RandomNumberGenerator.
        Random.Shared.NextBytes(randomBytes);

        ulong random64 = BitConverter.ToUInt64(randomBytes);

        // Take lower 62 bits
        UInt128 randomPart = random64 & ((1UL << RandomBits) - 1);

        UInt128 timestampPart = (UInt128)msSinceEpoch;

        // Combine: (Timestamp << 62) | Random
        UInt128 combined = (timestampPart << RandomBits) | randomPart;

        return new KairosId(combined);
    }

    /// <summary>
    /// Parses a string into a KairosId. Default format is Base58.
    /// </summary>
    public static KairosId Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("Invalid KairosId format.");
    }

    /// <summary>
    /// Tries to parse a string into a KairosId.
    /// Detects format based on length/content or assumes Base58.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out KairosId result
    )
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Tries to parse a span of characters into a KairosId.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out KairosId result
    )
    {
        switch (s.Length)
        {
            case 18 when Base58.TryDecode(s, out var val):
                result = new KairosId(val);
                return true;
            case 22 when Base32.TryDecode(s, out var val):
                result = new KairosId(val);
                return true;
            case 27 when Base16.TryDecode(s, out var val):
                result = new KairosId(val);
                return true;
            default:
                result = default;
                return false;
        }
    }

    /// <summary>
    /// Parses a span of characters into a KairosId.
    /// </summary>
    public static KairosId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("Invalid KairosId format.");
    }

    // Explicit parsing methods for clarity
    public static KairosId ParseBase58(ReadOnlySpan<char> s) =>
        Base58.TryDecode(s, out var v)
            ? new KairosId(v)
            : throw new FormatException("Invalid Base58");

    public static KairosId ParseBase32(ReadOnlySpan<char> s) =>
        Base32.TryDecode(s, out var v)
            ? new KairosId(v)
            : throw new FormatException("Invalid Base32");

    public static KairosId ParseHex(ReadOnlySpan<char> s) =>
        Base16.TryDecode(s, out var v)
            ? new KairosId(v)
            : throw new FormatException("Invalid Hex");

    // Equality
    public bool Equals(KairosId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is KairosId other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(KairosId left, KairosId right) => left.Equals(right);

    public static bool operator !=(KairosId left, KairosId right) => !left.Equals(right);

    // Comparable
    public int CompareTo(KairosId other) => _value.CompareTo(other._value);

    public static bool operator <(KairosId left, KairosId right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(KairosId left, KairosId right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(KairosId left, KairosId right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(KairosId left, KairosId right) =>
        left.CompareTo(right) >= 0;

    /// <summary>
    /// Returns a 16-element byte array that contains the value of this instance.
    /// </summary>
    public byte[] ToByteArray()
    {
        byte[] bytes = new byte[16];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(bytes, _value);
        return bytes;
    }

    // Formatting
    public override string ToString() => ToString(null, null);

    public string ToString(string? format) => ToString(format, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ReadOnlySpan<char> fmt = format;
        if (fmt.IsEmpty)
            fmt = "B58";

        switch (fmt)
        {
            case "B58":
            case "b58":
                return string.Create(
                    18,
                    _value,
                    (span, val) =>
                    {
                        Base58.TryEncode(val, span, out _);
                    }
                );
            case "B32":
            case "b32":
                return string.Create(
                    22,
                    _value,
                    (span, val) =>
                    {
                        Base32.TryEncode(val, span, out _);
                    }
                );
            case "B16":
            case "X":
            {
                return string.Create(
                    27,
                    _value,
                    (span, val) =>
                    {
                        Base16.TryEncode(val, span, upperCase: true, out _);
                    }
                );
            }
            case "b16":
            case "x":
            {
                return string.Create(
                    27,
                    _value,
                    (span, val) =>
                    {
                        Base16.TryEncode(val, span, upperCase: false, out _);
                    }
                );
            }

            default:
                throw new FormatException(
                    $"Unknown format '{format}'. Supported: B58, B32, B16."
                );
        }
    }

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        if (format.IsEmpty)
            format = "B58";

        switch (format)
        {
            case "B58":
            case "b58":
                return Base58.TryEncode(_value, destination, out charsWritten);
            case "B32":
            case "b32":
                return Base32.TryEncode(_value, destination, out charsWritten);
            case "B16":
            case "X":
                return Base16.TryEncode(
                    _value,
                    destination,
                    upperCase: true,
                    out charsWritten
                );
            case "b16":
            case "x":
                return Base16.TryEncode(
                    _value,
                    destination,
                    upperCase: false,
                    out charsWritten
                );
            default:
                charsWritten = 0;
                return false;
        }
    }

    // Explicit Instance Methods for Strings
    public string ToBase58() => ToString("B58");

    public string ToBase32() => ToString("B32");

    public string ToHex(bool upperCase = true) => ToString(upperCase ? "B16" : "b16");
}
