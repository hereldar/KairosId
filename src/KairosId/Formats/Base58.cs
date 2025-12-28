namespace KairosId.Formats;

internal static class Base58
{
    private const string Alphabet =
        "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly char[] AlphabetArray = Alphabet.ToCharArray();
    private static readonly byte[] DecodeMap = new byte[128];
    private static readonly UInt128[] PowersOf58 = new UInt128[18];

    static Base58()
    {
        Array.Fill(DecodeMap, (byte)255);
        for (int i = 0; i < Alphabet.Length; i++)
        {
            DecodeMap[Alphabet[i]] = (byte)i;
        }

        // Precompute powers of 58
        UInt128 currentPower = 1;
        for (int i = 0; i < 18; i++)
        {
            PowersOf58[i] = currentPower;
            currentPower *= 58;
        }
    }

    public static bool TryEncode(
        UInt128 value,
        Span<char> destination,
        out int charsWritten
    )
    {
        // 18 chars is enough for our specific range of 106-bit IDs (for ~70 years from epoch).
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
                // UInt128 supports / and % natively in .NET 7+
                UInt128 remainder = target % 58;
                target /= 58;
                destination[i] = AlphabetArray[(int)remainder];
            }
            else
            {
                destination[i] = AlphabetArray[0]; // Pad with '1'
            }
        }

        if (target > 0)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = 18;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        result = 0;
        if (source.IsEmpty || source.Length != 18)
            return false;

        // Optimized decoding using precomputed powers.
        // Input: "ABC..." (Most Significant first)
        // Value = A * 58^17 + B * 58^16 + ...

        // We can accumulate directly.
        // And we check for invalid characters.

        UInt128 acc = 0;

        for (int i = 0; i < 18; i++)
        {
            char c = source[i];
            if (c >= 128)
                return false;

            byte val = DecodeMap[c];
            if (val == 255)
                return false;

            // PowersOf58 is stored 0..17, where 0 is 58^0 (1)
            // The last character (index 17) corresponds to PowersOf58[0]
            // The first character (index 0) corresponds to PowersOf58[17]

            acc += (UInt128)val * PowersOf58[17 - i];
        }

        result = acc;
        return true;
    }
}
