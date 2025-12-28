namespace KairosId.Formats;

internal static class Base58
{
    private const string Alphabet =
        "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly char[] AlphabetArray = Alphabet.ToCharArray();
    private static readonly byte[] DecodeMap = new byte[128];
    private static readonly UInt128[] PowersOf58 = new UInt128[18];

    // 58^10 = 430,804,206,899,405,824 (fits in ulong)
    private static readonly ulong Divisor58Pow10 = 430804206899405824;

    static Base58()
    {
        Array.Fill(DecodeMap, (byte)255);
        for (int i = 0; i < Alphabet.Length; i++)
        {
            DecodeMap[Alphabet[i]] = (byte)i;
        }

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
        // 18 chars are required for 105-bit IDs
        if (destination.Length < 18)
        {
            charsWritten = 0;
            return false;
        }

        // Block-based encoding using 58^10 to allow fast 64-bit logic

        // Pass 1: Handle the lower 10 characters (Least Significant)
        UInt128 q = value / Divisor58Pow10;
        ulong r = (ulong)(value % Divisor58Pow10);
        EncodeBlock(r, destination, 17, 10);

        // Pass 2: Handle the upper 8 characters (Most Significant)
        // For 105-bit IDs, the quotient fits into a ulong
        EncodeBlock((ulong)q, destination, 7, 8);

        charsWritten = 18;
        return true;
    }

    private static void EncodeBlock(
        ulong value,
        Span<char> buffer,
        int endIndex,
        int count
    )
    {
        var v = value;
        for (int i = 0; i < count; i++)
        {
            ulong rem = v % 58;
            v /= 58;
            buffer[endIndex - i] = AlphabetArray[(int)rem];
        }
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        result = 0;
        if (source.IsEmpty || source.Length != 18)
        {
            return false;
        }

        // Unrolled decoding using precomputed powers
        UInt128 acc = 0;

        if (!TryAdd(source[0], 17, ref acc))
            return false;
        if (!TryAdd(source[1], 16, ref acc))
            return false;
        if (!TryAdd(source[2], 15, ref acc))
            return false;
        if (!TryAdd(source[3], 14, ref acc))
            return false;
        if (!TryAdd(source[4], 13, ref acc))
            return false;
        if (!TryAdd(source[5], 12, ref acc))
            return false;
        if (!TryAdd(source[6], 11, ref acc))
            return false;
        if (!TryAdd(source[7], 10, ref acc))
            return false;
        if (!TryAdd(source[8], 9, ref acc))
            return false;
        if (!TryAdd(source[9], 8, ref acc))
            return false;
        if (!TryAdd(source[10], 7, ref acc))
            return false;
        if (!TryAdd(source[11], 6, ref acc))
            return false;
        if (!TryAdd(source[12], 5, ref acc))
            return false;
        if (!TryAdd(source[13], 4, ref acc))
            return false;
        if (!TryAdd(source[14], 3, ref acc))
            return false;
        if (!TryAdd(source[15], 2, ref acc))
            return false;
        if (!TryAdd(source[16], 1, ref acc))
            return false;
        if (!TryAdd(source[17], 0, ref acc))
            return false;

        result = acc;
        return true;
    }

    private static bool TryAdd(char c, int powerIndex, ref UInt128 acc)
    {
        if (c >= 128)
            return false;
        byte val = DecodeMap[c];
        if (val == 255)
            return false;

        acc += (UInt128)val * PowersOf58[powerIndex];
        return true;
    }
}
