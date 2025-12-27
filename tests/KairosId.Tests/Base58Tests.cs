using KairosId.Formats;

namespace KairosId.Tests;

public class Base58Tests
{
    [Fact]
    public void TryEncode_Works()
    {
        UInt128 value = 0x1234567890ABCDEF;
        Span<char> destination = stackalloc char[18];
        bool success = Base58.TryEncode(value, destination, out int charsWritten);

        Assert.True(success);
        Assert.Equal(18, charsWritten);
    }

    [Fact]
    public void TryEncode_DestinationTooSmall_ReturnsFalse()
    {
        UInt128 value = 123;
        Span<char> destination = stackalloc char[17];
        bool success = Base58.TryEncode(value, destination, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryDecode_Valid_Works()
    {
        // 18 characters of '1' (which is 0 in the alphabet) should result in 0
        string input = new string('1', 18);
        bool success = Base58.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.True(success);
        Assert.Equal(UInt128.Zero, result);
    }

    [Fact]
    public void TryDecode_InvalidLength_ReturnsFalse()
    {
        string input = "123456";
        bool success = Base58.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void TryDecode_InvalidChars_ReturnsFalse()
    {
        // '0' (zero) is not in Base58 alphabet
        string input = "0123456789ABCDEFGH"; // 18 chars but has '0'
        bool success = Base58.TryDecode(input.AsSpan(), out UInt128 result);

        Assert.False(success);
    }

    [Fact]
    public void Roundtrip_Works()
    {
        UInt128 value = 0xABCDEF1234567890;
        Span<char> destination = stackalloc char[18];
        Base58.TryEncode(value, destination, out _);
        
        bool success = Base58.TryDecode(destination, out UInt128 result);
        Assert.True(success);
        Assert.Equal(value, result);
    }
}
