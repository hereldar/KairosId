using KairosId.Formats;

namespace KairosId.Tests;

public class Base32Tests
{
    [Fact]
    public void TryEncode_Works()
    {
        UInt128 value = 0x1234567890ABCDEF;
        Span<char> destination = stackalloc char[22];
        bool success = Base32.TryEncode(value, destination, out int charsWritten);

        Assert.True(success);
        Assert.Equal(22, charsWritten);
        // We can check some characters if we want to be precise, or just verify roundtrip
    }

    [Fact]
    public void TryEncode_DestinationTooSmall_ReturnsFalse()
    {
        UInt128 value = 123;
        Span<char> destination = stackalloc char[21];
        bool success = Base32.TryEncode(value, destination, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryDecode_Valid_Works()
    {
        // 22 characters of '1' (which is 1 in the alphabet)
        string input = new string('1', 22);
        bool success = Base32.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.True(success);
        // Not 1, but a huge number because each '1' is 5 bits shifted.
    }

    [Fact]
    public void TryDecode_Aliases_Work()
    {
        // O is 0, I is 1, L is 1 (lowercase allowed too)
        string input = "0123456789ABCDEFGHJKMN"; // 22 chars
        Base32.TryDecode(input.AsSpan(), out UInt128 expected);

        string aliased = "0123456789ABCDEFGHJKMN".Replace('0', 'O').Replace('1', 'I');
        bool success = Base32.TryDecode(aliased.AsSpan(), out UInt128 result);

        Assert.True(success);
        Assert.Equal(expected, result);

        string aliased2 = "0123456789ABCDEFGHJKMN".Replace('1', 'L').ToLower();
        success = Base32.TryDecode(aliased2.AsSpan(), out UInt128 result2);
        Assert.True(success);
        Assert.Equal(expected, result2);
    }

    [Fact]
    public void TryDecode_InvalidLength_ReturnsFalse()
    {
        string input = "ABC123";
        bool success = Base32.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void TryDecode_InvalidChars_ReturnsFalse()
    {
        // 'U' is not in Crockford's alphabet
        string input = new string('U', 22);
        bool success = Base32.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void Roundtrip_Works()
    {
        UInt128 value = 0x9876543210FEDCBA;
        Span<char> destination = stackalloc char[22];
        Base32.TryEncode(value, destination, out _);
        
        bool success = Base32.TryDecode(destination, out UInt128 result);
        Assert.True(success);
        Assert.Equal(value, result);
    }
}
