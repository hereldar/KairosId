namespace KairosId.Formats;

internal static class Base64
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    private static readonly char[] AlphabetArray = Alphabet.ToCharArray();
    private static readonly byte[] DecodeMap = new byte[128];

    static Base64()
    {
        Array.Fill(DecodeMap, (byte)255);
        for (int i = 0; i < Alphabet.Length; i++)
        {
            DecodeMap[Alphabet[i]] = (byte)i;
        }
    }

    public static bool TryEncode(UInt128 value, Span<char> destination, out int charsWritten)
    {
        if (destination.Length < 18)
        {
            charsWritten = 0;
            return false;
        }

        // 106 bits / 6 bits per char = 17.66 -> 18 characters.
        // We encode from right to left (Least Significant 6 bits -> last char).
        
        for (int i = 17; i >= 0; i--)
        {
            destination[i] = AlphabetArray[(int)(value & 63)];
            value >>>= 6;
        }

        charsWritten = 18;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<char> source, out UInt128 result)
    {
        if (source.Length != 18)
        {
            result = 0;
            return false;
        }

        result = 0;
        foreach (char c in source)
        {
            if (c >= 128) return false;
            byte val = DecodeMap[c];
            if (val == 255) return false;

            result = (result << 6) | val;
        }

        return true;
    }
}
