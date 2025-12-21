using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security.Cryptography;
using KairosID.Formats;

namespace KairosID;

/// <summary>
/// Represents a 106-bit identifier composed of a 48-bit timestamp and 58-bit randomness.
/// The identifier is designed to be time-ordered (k-sorted).
/// Default string representation is Base58 (18 characters).
/// </summary>
public readonly struct KairosId : IEquatable<KairosId>, IComparable<KairosId>, IParsable<KairosId>, ISpanParsable<KairosId>, ISpanFormattable
{
    private const int TimestampBits = 48;
    private const int RandomBits = 58;
    private const long EpochTimestamp = 1577836800000; // Jan 1 2020 UTC in ms
    private const int EncodedLength = 18; // Base58 length

    // Mask for 58 bits of randomness
    private static readonly UInt128 RandomMask = (UInt128.One << RandomBits) - 1;

    // The internal 128-bit storage.
    // Layout (Big Endian conceptual view): [00...00 (22 bits)] [Timestamp (48 bits)] [Random (58 bits)]
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
            // Shift down 58 bits to get timestamp
            long timestampOffset = (long)(_value >> RandomBits);
            return DateTimeOffset.FromUnixTimeMilliseconds(timestampOffset + EpochTimestamp);
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
    /// Generates a new unique KairosId.
    /// </summary>
    /// <returns>A new KairosId.</returns>
    [Obsolete("Use NewKairosId instead.")]
    public static KairosId NewId()
    {
        return NewKairosId();
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
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp must be after Jan 1 2020.");
        }
        
        // 48 bits check: 2^48 ms is approx 8925 years.
        // But let's check explicit max if needed.
        if (msSinceEpoch >= (1L << TimestampBits))
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp too far in the future.");
        }

        // Generate 58 bits of randomness
        // We need 8 bytes (64 bits) and mask it.
        Span<byte> randomBytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        ulong random64 = BitConverter.ToUInt64(randomBytes);
        
        // Take lower 58 bits
        UInt128 randomPart = random64 & ((1UL << RandomBits) - 1);
        
        UInt128 timestampPart = (UInt128)msSinceEpoch;
        
        // Combine: (Timestamp << 58) | Random
        UInt128 combined = (timestampPart << RandomBits) | randomPart;
        
        return new KairosId(combined);
    }

    /// <summary>
    /// Parses a string into a KairosId. Default format is Base58.
    /// </summary>
    public static KairosId Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException("Invalid KairosId format.");
        }
        return result;
    }

    /// <summary>
    /// Tries to parse a string into a KairosId.
    /// Detects format based on length/content or assumes Base58.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out KairosId result)
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
    /// <summary>
    /// Tries to parse a span of characters into a KairosId.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out KairosId result)
    {
        // Simple heuristic detection based on length
        // Base58 length is 18.
        // Base32 length is 22.
        // Hex length is 27 (usually) or 32 for full UInt128.
        
        if (s.Length == 18)
        {
             if (Formats.Base58.TryDecode(s, out var val))
             {
                 result = new KairosId(val);
                 return true;
             }
        }
        else if (s.Length == 22) // Base32
        {
            if (Formats.Base32.TryDecode(s, out var val))
            {
                result = new KairosId(val);
                return true;
            }
        }
        else if (s.Length == 32 || s.Length == 27) // Hex
        {
            if (UInt128.TryParse(s, System.Globalization.NumberStyles.HexNumber, provider, out var val))
            {
                result = new KairosId(val);
                return true;
            }
        }
        else if (s.Length == 24) // Base64 (128 bits -> 16 bytes -> 24 chars with padding)
        {
            if (TryParseBase64(s, out result))
            {
                return true;
            }
        }
        
        result = default;
        return false;
    }

    /// <summary>
    /// Parses a span of characters into a KairosId.
    /// </summary>
    public static KairosId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException("Invalid KairosId format.");
        }
        return result;
    }
    
    // Explicit parsing methods for clarity
    public static KairosId ParseBase58(ReadOnlySpan<char> s) => Formats.Base58.TryDecode(s, out var v) ? new KairosId(v) : throw new FormatException("Invalid Base58");
    public static KairosId ParseBase32(ReadOnlySpan<char> s) => Formats.Base32.TryDecode(s, out var v) ? new KairosId(v) : throw new FormatException("Invalid Base32");
    public static KairosId ParseHex(ReadOnlySpan<char> s) => UInt128.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out var v) ? new KairosId(v) : throw new FormatException("Invalid Hex");
    public static KairosId ParseBase64(ReadOnlySpan<char> s) => TryParseBase64(s, out var v) ? v : throw new FormatException("Invalid Base64");

    public static bool TryParseBase64(ReadOnlySpan<char> s, out KairosId result)
    {
        Span<byte> bytes = stackalloc byte[16];
        if (Convert.TryFromBase64Chars(s, bytes, out int bytesWritten) && bytesWritten == 16)
        {
            result = new KairosId(System.Buffers.Binary.BinaryPrimitives.ReadUInt128BigEndian(bytes));
            return true;
        }
        result = default;
        return false;
    }

    // Equality
    public bool Equals(KairosId other) => _value == other._value;
    public override bool Equals(object? obj) => obj is KairosId other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(KairosId left, KairosId right) => left.Equals(right);
    public static bool operator !=(KairosId left, KairosId right) => !left.Equals(right);

    // Comparable
    public int CompareTo(KairosId other) => _value.CompareTo(other._value);
    public static bool operator <(KairosId left, KairosId right) => left.CompareTo(right) < 0;
    public static bool operator <=(KairosId left, KairosId right) => left.CompareTo(right) <= 0;
    public static bool operator >(KairosId left, KairosId right) => left.CompareTo(right) > 0;
    public static bool operator >=(KairosId left, KairosId right) => left.CompareTo(right) >= 0;

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
        // Allocation-free-ish path?
        // We have to return a string, so allocation happens.
        // We can use string.Create
        
        format = string.IsNullOrEmpty(format) ? "B58" : format.ToUpperInvariant();

        switch (format)
        {
            case "B58":
                return string.Create(18, _value, (span, val) => 
                {
                    Formats.Base58.TryEncode(val, span, out _);
                });
            case "B32":
                 return string.Create(22, _value, (span, val) => 
                {
                    Formats.Base32.TryEncode(val, span, out _);
                });
            case "B16":
            case "X":
                // Hex
                return _value.ToString("X27", formatProvider); // 106 bits is ~27 hex digits (26.5)
            case "B64":
                // Base64
                // We'll treat the 14 needed bytes ??
                // Let's do full 16 bytes for generic safety.
                byte[] bytes = new byte[16];
                // Write Big Endian
                // UInt128.WriteBigEndian works? Not in .NET 7/8 standard?
                // BinaryPrimitives has WriteUInt128BigEndian in .NET 8
                System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(bytes, _value);
                return Convert.ToBase64String(bytes);
                
            default:
                throw new FormatException($"Unknown format '{format}'. Supported: B58, B32, B16, B64.");
        }
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        // Default B58
        if (format.IsEmpty) format = "B58";
        
        // Simple switch on first char for speed?
        char f = char.ToUpperInvariant(format[0]);
        
        if (format.Equals("B58", StringComparison.OrdinalIgnoreCase))
        {
            return Formats.Base58.TryEncode(_value, destination, out charsWritten);
        }
        else if (format.Equals("B32", StringComparison.OrdinalIgnoreCase))
        {
             return Formats.Base32.TryEncode(_value, destination, out charsWritten);
        }
        else if (format.Equals("B16", StringComparison.OrdinalIgnoreCase) || f == 'X' || f == 'N' || f == 'D')
        {
            // For N and D we match Guid behavior (no extras, hyphens etc not supported for now in custom bits)
            // But let's just use X27 or X32.
            string fmt = (f == 'X') ? "X" : "X32"; 
            return _value.TryFormat(destination, out charsWritten, fmt, provider);
        }
        else if (format.Equals("B64", StringComparison.OrdinalIgnoreCase))
        {
             // Span-based Base64?
             // Need bytes first.
             Span<byte> bytes = stackalloc byte[16];
             System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(bytes, _value);
             return Convert.TryToBase64Chars(bytes, destination, out charsWritten);
        }

        charsWritten = 0;
        return false;
    }

    // Explicit Instance Methods for Strings
    public string ToBase58() => ToString("B58");
    public string ToBase32() => ToString("B32");
    public string ToHex() => ToString("B16");
    public string ToBase64() => ToString("B64");
}
