using System;
using System.Buffers;
using System.Numerics;

namespace KairosID.Formats;

internal static class Base58
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly char[] AlphabetArray = Alphabet.ToCharArray();
    private static readonly byte[] DecodeMap = new byte[128];

    static Base58()
    {
        Array.Fill(DecodeMap, (byte)255);
        for (int i = 0; i < Alphabet.Length; i++)
        {
            DecodeMap[Alphabet[i]] = (byte)i;
        }
    }

    public static bool TryEncode(UInt128 value, Span<char> destination, out int charsWritten)
    {
        // 18 chars is enough for our specific range of 106-bit IDs (for ~70 years from epoch).
        // However, generic UInt128 might need more. Max UInt128 is ~3.4e38, 58^22 is ~5.5e38.
        // So safe buffer size is 22.
        if (destination.Length < 18)
        {
            charsWritten = 0;
            return false;
        }

        var target = value;

        
        // This loop works backwards from the end of the buffer
        for (int i = 17; i >= 0; i--)
        {
            if (target > 0)
            {
                (target, UInt128 remainder) = DivRem(target, 58);
                destination[i] = AlphabetArray[(int)remainder];
            }
            else
            {
                destination[i] = AlphabetArray[0]; // Pad with '1'
            }
        }
        
        // Check if value was too large to fit in 18 chars
        if (target > 0)
        {
             // Although for our KairosID use case this shouldn't happen if valid,
             // for a generic Base58 it might. But here we enforce fixed 18 length layout.
             // If it overflows, we fail? Or we require larger buffer?
             // Since this is internal for KairosId which mandates 18 chars:
             charsWritten = 0;
             return false;
        }

        charsWritten = 18;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        result = 0;
        if (source.IsEmpty) return false;

        UInt128 factor = 1;
        
        // Process from right to left? Or left to right?
        // Standard Base58 is big-endian usually (leading '1's are leading zeros).
        // Previous logic: result * 58 + digit. This is left-to-right (MSB first).
        
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c >= 128 || DecodeMap[c] == 255)
            {
                return false;
            }
            
            // Check overflow before multiply? 
            // result * 58 + digit
            // Since we use UInt128, and we expect 106 bits approx, it fits easily.
            // But good to be safe if `source` is long.
            
            result = result * 58 + DecodeMap[c];
        }

        return true;
    }
    
    // Helper until .NET 8 UInt128 / operator optimization is verified or if we want explict DivRem
    private static (UInt128 Quotient, UInt128 Remainder) DivRem(UInt128 left, UInt128 right)
    {
        return (left / right, left % right);
    }
}
