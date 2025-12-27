using KairosId.Formats;

namespace KairosId.Tests;

public class Base64Tests
{
    [Fact]
    public void Encode_Decode_Roundtrip()
    {
        UInt128 original = (UInt128.One << 105) | 12345;
        Span<char> encoded = stackalloc char[18];
        
        Assert.True(Base64.TryEncode(original, encoded, out int written));
        Assert.Equal(18, written);
        
        Assert.True(Base64.TryDecode(encoded, out UInt128 decoded));
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void Encode_Zero_Works()
    {
        UInt128 original = 0;
        Span<char> encoded = stackalloc char[18];
        
        Assert.True(Base64.TryEncode(original, encoded, out _));
        Assert.Equal("AAAAAAAAAAAAAAAAAA", encoded.ToString());
        
        Assert.True(Base64.TryDecode(encoded, out UInt128 decoded));
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void Decode_InvalidChars_ReturnsFalse()
    {
        Assert.False(Base64.TryDecode("InvalidChars!@#$%^", out _));
    }

    [Fact]
    public void Decode_WrongLength_ReturnsFalse()
    {
        Assert.False(Base64.TryDecode("TooShort", out _));
        Assert.False(Base64.TryDecode("TooLong1234567890123", out _));
    }
}
