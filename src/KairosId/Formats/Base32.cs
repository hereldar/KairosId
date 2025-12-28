namespace KairosId.Formats;

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

    public static bool TryEncode(
        UInt128 value,
        Span<char> destination,
        out int charsWritten
    )
    {
        if (destination.Length < 22)
        {
            charsWritten = 0;
            return false;
        }

        // Unrolled for 22 characters (106 bits covers ~21.2 chars, so 22 is correct padding)
        // 5 bits per char.
        // We act on the value directly.
        // To be consistent with the loop `remainder = target & 31; target >>>= 5; dest[len - 1 - i] = x`
        // checks from right to left (Least Significant 5 bits -> Last char).

        // char 21 (last)
        destination[21] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 20
        destination[20] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 19
        destination[19] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 18
        destination[18] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 17
        destination[17] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 16
        destination[16] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 15
        destination[15] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 14
        destination[14] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 13
        destination[13] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 12
        destination[12] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 11
        destination[11] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 10
        destination[10] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 9
        destination[9] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 8
        destination[8] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 7
        destination[7] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 6
        destination[6] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 5
        destination[5] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 4
        destination[4] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 3
        destination[3] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 2
        destination[2] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 1
        destination[1] = AlphabetArray[(int)(value & 31)];
        value >>>= 5;
        // char 0 (first)
        destination[0] = AlphabetArray[(int)(value & 31)];

        charsWritten = 22;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        // Fixed length check for optimal performance?
        // Or unroll with fallback? KairosId expects 22 chars for Base32.
        if (source.Length != 22)
        {
            result = 0;
            return false;
        }

        // We can just look up all bytes and combine them.
        // We'll trust the DecodeMap returns 255 for invalid.
        // But verifying *every* char is expensive if we do branches.
        // OR'ing the results and checking at the end is faster, but we need to check 255.
        // Since 255 has high bit set (0xFF), if OR'd together, we might detect it?
        // 5 bits max is 31 (0x1F). So 0xFF is distinguishable.

        // Let's do a safe unroll.

        // char 0 -> top bits.
        // char 21 -> bottom bits.

        // We accumulate into UInt128.
        // Since we are shifting 5 bits at a time.
        // result = (result << 5) | val;

        // Parallel construction:
        // (val0 << 105) | (val1 << 100) ...

        // Let's load them to local vars to avoid bounds checks if JIT doesn't elide them (it should for fixed length).

        byte c0 = DecodeMap[source[0]];
        byte c1 = DecodeMap[source[1]];
        byte c2 = DecodeMap[source[2]];
        byte c3 = DecodeMap[source[3]];
        byte c4 = DecodeMap[source[4]];
        byte c5 = DecodeMap[source[5]];
        byte c6 = DecodeMap[source[6]];
        byte c7 = DecodeMap[source[7]];
        byte c8 = DecodeMap[source[8]];
        byte c9 = DecodeMap[source[9]];
        byte c10 = DecodeMap[source[10]];
        byte c11 = DecodeMap[source[11]];
        byte c12 = DecodeMap[source[12]];
        byte c13 = DecodeMap[source[13]];
        byte c14 = DecodeMap[source[14]];
        byte c15 = DecodeMap[source[15]];
        byte c16 = DecodeMap[source[16]];
        byte c17 = DecodeMap[source[17]];
        byte c18 = DecodeMap[source[18]];
        byte c19 = DecodeMap[source[19]];
        byte c20 = DecodeMap[source[20]];
        byte c21 = DecodeMap[source[21]];

        // If any is 255, fail.
        if (
            (
                c0
                | c1
                | c2
                | c3
                | c4
                | c5
                | c6
                | c7
                | c8
                | c9
                | c10
                | c11
                | c12
                | c13
                | c14
                | c15
                | c16
                | c17
                | c18
                | c19
                | c20
                | c21
            ) == 255
        )
        {
            result = 0;
            return false;
        }

        result =
            ((UInt128)c0 << 105)
            | ((UInt128)c1 << 100)
            | ((UInt128)c2 << 95)
            | ((UInt128)c3 << 90)
            | ((UInt128)c4 << 85)
            | ((UInt128)c5 << 80)
            | ((UInt128)c6 << 75)
            | ((UInt128)c7 << 70)
            | ((UInt128)c8 << 65)
            | ((UInt128)c9 << 60)
            | ((UInt128)c10 << 55)
            | ((UInt128)c11 << 50)
            | ((UInt128)c12 << 45)
            | ((UInt128)c13 << 40)
            | ((UInt128)c14 << 35)
            | ((UInt128)c15 << 30)
            | ((UInt128)c16 << 25)
            | ((UInt128)c17 << 20)
            | ((UInt128)c18 << 15)
            | ((UInt128)c19 << 10)
            | ((UInt128)c20 << 5)
            | ((UInt128)c21);

        return true;
    }
}
