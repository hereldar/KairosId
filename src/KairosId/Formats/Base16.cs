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

    public static bool TryEncode(UInt128 value, Span<char> destination, bool upperCase, out int charsWritten)
    {
        // KairosId is 106 bits.
        // 106 bits / 4 bits per hex char = 26.5 chars -> 27 chars.
        // Let's output 27 chars to be safe/consistent with "X27" formatting, 
        // OR we could do 32 chars (full UInt128) and trim leading zeros if desired.
        // The previous implementation used "X27".
        
        // We will target 27 characters (padding with 0 if needed for the top bits).
        
        int length = 27;
        if (destination.Length < length)
        {
            charsWritten = 0;
            return false;
        }

        char[] table = upperCase ? HexTableUpper : HexTableLower;
        
        // We process 4 bits at a time from properties of value.
        // Unrolling slightly for performance.
        
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
        // We allow variable length parsing essentially, but for KairosId strictness
        // we might usually expect 27 or 32 chars.
        // Let's implement a generic hex parser for UInt128.
        
        result = 0;
        if (source.IsEmpty) return false;

        // Skip '0x' if present? (Guid.Parse doesn't usually require it, checking KairosId logic)
        // KairosId.TryParse checked specific lengths.
        
        // We accumulate result.
        
        // Check for max 32 chars (128 bits). 
        if (source.Length > 32) return false; 
        
        UInt128 acc = 0;
        foreach (char c in source)
        {
            if (c >= 128) return false;
            byte val = DecodeMap[c];
            if (val == 255) return false;
            
            acc = (acc << 4) | val;
        }
        
        result = acc;
        return true;
    }
}
