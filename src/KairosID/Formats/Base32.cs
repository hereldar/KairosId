using System;

namespace KairosID.Formats;

internal static class Base32
{
    // Crockford's Base32 Alphabet
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private static readonly char[] AlphabetArray = Alphabet.ToCharArray();
    private static readonly byte[] DecodeMap = new byte[128];

    static Base32()
    {
        Array.Fill(DecodeMap, (byte)255);
        for (int i = 0; i < Alphabet.Length; i++)
        {
            DecodeMap[Alphabet[i]] = (byte)i;
            // Handle lowercase
            char c = Alphabet[i];
            if (char.IsLetter(c))
            {
                DecodeMap[char.ToLower(c)] = (byte)i;
            }
        }
        
        // Handle aliases (O=0, I=1, L=1) commonly used in Crockford's
        DecodeMap['O'] = 0;
        DecodeMap['o'] = 0;
        DecodeMap['I'] = 1;
        DecodeMap['i'] = 1;
        DecodeMap['L'] = 1;
        DecodeMap['l'] = 1;
    }

    public static bool TryEncode(UInt128 value, Span<char> destination, out int charsWritten)
    {
        // 106 bits / 5 bits per char = 21.2 -> 22 chars.
        // We'll require enough space.
        if (destination.Length < 22)
        {
            charsWritten = 0;
            return false;
        }

        var target = value;
        // Unlike Base58 which is fixed 18 in this library, Base32 might vary?
        // Let's output variable length or fixed?
        // Base58 implementation fixed it to 18 padding with '1's.
        // For Base32, consistency suggests padding with '0's to a fixed length?
        // 106 bits fits in 22 chars.
        
        int len = 22;
        int index = len - 1;

        for (int i = 0; i < len; i++)
        {
             // target % 32 is just target & 31
             int remainder = (int)(target & 31);
             destination[len - 1 - i] = AlphabetArray[remainder];
             target >>>= 5; // Efficient bit shift for power of 2 base
        }

        charsWritten = len;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        result = 0;
        if (source.IsEmpty) return false;

        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c >= 128 || DecodeMap[c] == 255)
            {
                return false;
            }
            
            result = (result << 5) | DecodeMap[c];
        }

        return true;
    }
}
