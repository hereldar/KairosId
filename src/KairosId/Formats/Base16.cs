namespace KairosId.Formats;

internal static class Base16
{
    private static readonly char[] HexTableUpper = "0123456789ABCDEF".ToCharArray();
    private static readonly char[] HexTableLower = "0123456789abcdef".ToCharArray();

    // Reverse map for decoding (supports both upper and lower)
    private static readonly byte[] DecodeMap = new byte[128];

    static Base16()
    {
        Array.Fill(DecodeMap, (byte)255);

        for (int i = 0; i < 10; i++)
        {
            DecodeMap['0' + i] = (byte)i;
        }
        for (int i = 0; i < 6; i++)
        {
            DecodeMap['A' + i] = (byte)(10 + i);
            DecodeMap['a' + i] = (byte)(10 + i);
        }
    }

    public static bool TryEncode(
        UInt128 value,
        Span<char> destination,
        bool upperCase,
        out int charsWritten
    )
    {
        // KairosId is 106 bits.
        // 106 bits / 4 bits per hex char = 26.5 chars -> 27 chars.
        // We output 27 chars to be consistent with the identifier size.

        const int length = 27;
        if (destination.Length < length)
        {
            charsWritten = 0;
            return false;
        }

        char[] table = upperCase ? HexTableUpper : HexTableLower;

        var v = value;
        for (int i = length - 1; i >= 0; i--)
        {
            destination[i] = table[(int)(v & 0xF)];
            v >>= 4;
        }

        charsWritten = length;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        result = 0;
        if (source.Length != 27)
            return false;

        UInt128 acc = 0;
        foreach (char c in source)
        {
            if (c >= 128)
                return false;
            byte val = DecodeMap[c];
            if (val == 255)
                return false;

            acc = (acc << 4) | val;
        }

        result = acc;
        return true;
    }
}
