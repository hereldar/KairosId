namespace KairosId.Tests;

public class KairosIdTests
{
    [Fact]
    public void NewKairosId_GeneratesUniqueIds()
    {
        var id1 = KairosId.NewKairosId();
        var id2 = KairosId.NewKairosId();
        
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void NewKairosId_IsTimeOrdered()
    {
        // Generate two IDs with a slight delay
        var id1 = KairosId.NewKairosId();
        Thread.Sleep(10); // Ensure clock ticks
        var id2 = KairosId.NewKairosId();
        
        Assert.True(id1 < id2);
        Assert.True(id1.Timestamp < id2.Timestamp);
    }

    [Fact]
    public void Empty_IsZero()
    {
        var id = KairosId.Empty;
        Assert.Equal(UInt128.Zero, id.Value);
    }

    [Fact]
    public void ToByteArray_CorrectLayout()
    {
        var id = KairosId.NewKairosId();
        byte[] bytes = id.ToByteArray();
        
        Assert.Equal(16, bytes.Length);
        UInt128 recovered = System.Buffers.Binary.BinaryPrimitives.ReadUInt128BigEndian(bytes);
        Assert.Equal(id.Value, recovered);
    }

    [Fact]
    public void ToString_Default_IsBase58()
    {
        var id = KairosId.NewKairosId();
        string s = id.ToString();
        
        Assert.Equal(18, s.Length);
        // Base58 shouldn't contain 0, O, I, l
        Assert.DoesNotContain("0", s);
        Assert.DoesNotContain("O", s);
        Assert.DoesNotContain("I", s);
        Assert.DoesNotContain("l", s);
    }

    [Fact]
    public void ExplicitFormatting_Methods_Work()
    {
        var id = KairosId.NewKairosId();
        
        Assert.Equal(id.ToString("B58"), id.ToBase58());
        Assert.Equal(id.ToString("B32"), id.ToBase32());
        Assert.Equal(id.ToString("B16"), id.ToHex());
        Assert.Equal(id.ToString("B64"), id.ToBase64());
    }

    [Fact]
    public void Formats_WorkAndRoundtrip()
    {
        var id = KairosId.NewKairosId();
        
        // Base58
        string b58 = id.ToBase58();
        Assert.Equal(18, b58.Length);
        Assert.Equal(id, KairosId.Parse(b58));
        
        // Base32
        string b32 = id.ToBase32();
        Assert.Equal(22, b32.Length);
        Assert.Equal(id, KairosId.ParseBase32(b32));
        
        // Hex
        string b16 = id.ToHex();
        Assert.Equal(27, b16.Length);
        Assert.Equal(id, KairosId.ParseHex(b16));
        
        // Base64
        string b64 = id.ToBase64();
        Assert.Equal(18, b64.Length);
        Assert.Equal(id, KairosId.ParseBase64(b64));
    }

    [Fact]
    public void Parse_AutoDetectsFormat()
    {
        var id = KairosId.NewKairosId();
        
        Assert.Equal(id, KairosId.Parse(id.ToBase58()));
        Assert.Equal(id, KairosId.Parse(id.ToBase32()));
        Assert.Equal(id, KairosId.Parse(id.ToHex()));
        Assert.Equal(id, KairosId.Parse(id.ToBase64()));
    }

    [Fact]
    public void ISpanParsable_Works()
    {
        var id = KairosId.NewKairosId();
        ReadOnlySpan<char> span = id.ToBase58().AsSpan();
        
        bool success = KairosId.TryParse(span, null, out var result);
        Assert.True(success);
        Assert.Equal(id, result);
    }

    [Fact]
    public void Timestamp_Extraction_Correct()
    {
        var now = DateTimeOffset.UtcNow;
        // Truncate to ms precision
        now = DateTimeOffset.FromUnixTimeMilliseconds(now.ToUnixTimeMilliseconds());
        
        var id = KairosId.NewKairosId(now);
        
        Assert.Equal(now, id.Timestamp);
    }

    [Fact]
    public void SortOrder_IsCorrect()
    {
        var t1 = DateTimeOffset.UtcNow;
        var t2 = t1.AddMilliseconds(1);
        
        var id1 = KairosId.NewKairosId(t1);
        var id2 = KairosId.NewKairosId(t2);
        
        Assert.True(id1.CompareTo(id2) < 0);
    }
    
    [Theory]
    [InlineData("111111111111111111")] // Min value
    public void BoundaryValues_Base58(string input)
    {
        var id = KairosId.Parse(input);
        Assert.Equal(UInt128.Zero, id.Value);
        Assert.Equal(input, id.ToString());
    }

    [Fact]
    public void Equality_And_Comparison_Operators_Work()
    {
        var id1 = KairosId.NewKairosId();
        Thread.Sleep(1);
        var id2 = KairosId.NewKairosId();
        var id1Copy = KairosId.Parse(id1.ToString());

        // Equality
        Assert.True(id1 == id1Copy);
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
        Assert.False(id1 != id1Copy);

        // Comparison
        Assert.True(id1 < id2);
        Assert.True(id1 <= id2);
        Assert.True(id1 <= id1Copy);
        Assert.True(id2 > id1);
        Assert.True(id2 >= id1);
        Assert.True(id1 >= id1Copy);
    }

    [Fact]
    public void GetHashCode_IsConsistent()
    {
        var id1 = KairosId.NewKairosId();
        var id1Copy = KairosId.Parse(id1.ToString());

        Assert.Equal(id1.GetHashCode(), id1Copy.GetHashCode());
    }

    [Fact]
    public void TryParse_InvalidFormats_ReturnsFalse()
    {
        Assert.False(KairosId.TryParse("TooShort", null, out _));
        Assert.False(KairosId.TryParse("TooLong123456789012345678901234567890", null, out _));
        Assert.False(KairosId.TryParse("InvalidChars!!@@##", null, out _));
        Assert.False(KairosId.TryParse((string?)null, null, out _));
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => KairosId.Parse("Invalid"));
    }

    [Fact]
    public void NewKairosId_SpecificDate_Works()
    {
        var date = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var id = KairosId.NewKairosId(date);
        
        Assert.Equal(date, id.Timestamp);
    }

    [Fact]
    public void NewKairosId_PastDate_ThrowsArgumentOutOfRangeException()
    {
        var pastDate = new DateTimeOffset(2019, 12, 31, 23, 59, 59, TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => KairosId.NewKairosId(pastDate));
    }

    [Theory]
    [InlineData("x")]
    [InlineData("b16")]
    public void Hex_Formatting_Supports_Lowercase(string format)
    {
        var id = KairosId.ParseHex("00000000000ABCDEF1234567890");
        
        // ToString
        Assert.Equal("00000000000abcdef1234567890", id.ToString(format));
        
        // TryFormat
        Span<char> span = stackalloc char[27];
        Assert.True(id.TryFormat(span, out _, format, null));
        Assert.Equal("00000000000abcdef1234567890", span.ToString());
    }

    [Fact]
    public void ToHex_Supports_Casing()
    {
        var id = KairosId.ParseHex("00000000000ABCDEF1234567890");
        Assert.Equal("00000000000ABCDEF1234567890", id.ToHex(true));
        Assert.Equal("00000000000abcdef1234567890", id.ToHex(false));
    }
}
