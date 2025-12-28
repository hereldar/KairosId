using KairosId.Formats;

namespace KairosId.Tests;

public class Base16Tests
{
    [Fact]
    public void TryEncode_Uppercase_Works()
    {
        UInt128 value = 0xABCDEF1234567890;
        Span<char> destination = stackalloc char[27];
        bool success = Base16.TryEncode(value, destination, true, out int charsWritten);

        Assert.True(success);
        Assert.Equal(27, charsWritten);
        Assert.Equal("00000000000ABCDEF1234567890", destination.ToString());
    }

    [Fact]
    public void TryEncode_Lowercase_Works()
    {
        UInt128 value = 0xABCDEF1234567890;
        Span<char> destination = stackalloc char[27];
        bool success = Base16.TryEncode(value, destination, false, out int charsWritten);

        Assert.True(success);
        Assert.Equal(27, charsWritten);
        Assert.Equal("00000000000abcdef1234567890", destination.ToString());
    }

    [Fact]
    public void TryEncode_DestinationTooSmall_ReturnsFalse()
    {
        UInt128 value = 123;
        Span<char> destination = stackalloc char[26];
        bool success = Base16.TryEncode(value, destination, true, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryDecode_ValidUpper_Works()
    {
        string input = "00000000000ABCDEF1234567890";
        bool success = Base16.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.True(success);
        Assert.Equal((UInt128)0xABCDEF1234567890, result);
    }

    [Fact]
    public void TryDecode_ValidLower_Works()
    {
        string input = "00000000000abcdef1234567890";
        bool success = Base16.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.True(success);
        Assert.Equal((UInt128)0xABCDEF1234567890, result);
    }

    [Fact]
    public void TryDecode_MixedCase_Works()
    {
        string input = "00000000000AbCdEf1234567890";
        bool success = Base16.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.True(success);
        Assert.Equal((UInt128)0xABCDEF1234567890, result);
    }

    [Fact]
    public void TryDecode_EmptySource_ReturnsFalse()
    {
        bool success = Base16.TryDecode(ReadOnlySpan<char>.Empty, out UInt128 result);

        Assert.False(success);
        Assert.Equal(UInt128.Zero, result);
    }

    [Fact]
    public void TryDecode_TooLong_ReturnsFalse()
    {
        string input = new string('F', 33);
        bool success = Base16.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void TryDecode_InvalidChars_ReturnsFalse()
    {
        string input = "GHIJKL";
        bool success = Base16.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void Roundtrip_Works()
    {
        UInt128 value = 0x1234567890ABCDEF;
        Span<char> destination = stackalloc char[27];
        Base16.TryEncode(value, destination, true, out _);

        bool success = Base16.TryDecode(destination, out UInt128 result);
        Assert.True(success);
        Assert.Equal(value, result);
    }
}
