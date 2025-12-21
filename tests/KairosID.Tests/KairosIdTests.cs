using System.Numerics;
using Xunit;

namespace KairosID.Tests;

public class KairosIdTests
{
    [Fact]
    public void NewId_GeneratesUniqueIds()
    {
        var id1 = KairosId.NewId();
        var id2 = KairosId.NewId();
        
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void NewId_IsTimeOrdered()
    {
        // Generate two IDs with a slight delay
        var id1 = KairosId.NewId();
        Thread.Sleep(10); // Ensure clock ticks
        var id2 = KairosId.NewId();
        
        Assert.True(id1 < id2);
        Assert.True(id1.Timestamp < id2.Timestamp);
    }

    [Fact]
    public void ToString_Default_IsBase58()
    {
        var id = KairosId.NewId();
        string s = id.ToString();
        
        Assert.Equal(18, s.Length);
        // Base58 shouldn't contain 0, O, I, l
        Assert.DoesNotContain("0", s);
        Assert.DoesNotContain("O", s);
        Assert.DoesNotContain("I", s);
        Assert.DoesNotContain("l", s);
    }

    [Fact]
    public void Formats_WorkAndRoundtrip()
    {
        var id = KairosId.NewId();
        
        // Base58
        string b58 = id.ToString("B58");
        Assert.Equal(18, b58.Length);
        Assert.Equal(id, KairosId.Parse(b58));
        
        // Base32
        string b32 = id.ToString("B32");
        Assert.Equal(22, b32.Length);
        var parsed32 = KairosId.ParseBase32(b32);
        Assert.Equal(id, parsed32);
        
        // Hex
        string b16 = id.ToString("B16");
        // Length should be >= 27
        Assert.True(b16.Length >= 27);
        var parsed16 = KairosId.ParseHex(b16);
        Assert.Equal(id, parsed16);
        
        // Base64
        string b64 = id.ToString("B64");
        // 16 bytes -> 24 chars (padded)
        Assert.Equal(24, b64.Length);
        // We don't have a direct ParseBase64 helper exposed in KairosId yet, 
        // but we can test semi-manually or add ParseBase64?
        // Actually typical Parse() logic doesn't detect B64 easily vs others without hints
        // unless regex.
        // But let's verify decoding manually via span
        Span<byte> bytes = stackalloc byte[16];
        Convert.TryFromBase64String(b64, bytes, out _);
        UInt128 val = System.Buffers.Binary.BinaryPrimitives.ReadUInt128BigEndian(bytes);
        Assert.Equal(id.Value, val);
    }

    [Fact]
    public void Parse_AutoDetectsFormat()
    {
        var id = KairosId.NewId();
        
        // Base58 (18 chars)
        Assert.Equal(id, KairosId.Parse(id.ToString()));
        
        // Base32 (22 chars)
        Assert.Equal(id, KairosId.Parse(id.ToString("B32")));
        
        // Hex detection isn't implemented in generic TryParse unless length matches?
        // My implementation only detects length 18 and 22 currently.
        // So Hex won't auto-parse via generic Parse().
    }

    [Fact]
    public void Timestamp_Extraction_Correct()
    {
        var now = DateTimeOffset.UtcNow;
        // Truncate to ms precision as KairosId is ms precise
        now = DateTimeOffset.FromUnixTimeMilliseconds(now.ToUnixTimeMilliseconds());
        
        var id = KairosId.NewId(now);
        
        Assert.Equal(now, id.Timestamp);
    }

    [Fact]
    public void SortOrder_IsCorrect()
    {
        var t1 = DateTimeOffset.UtcNow;
        var t2 = t1.AddMilliseconds(1);
        
        var id1 = KairosId.NewId(t1);
        var id2 = KairosId.NewId(t2);
        
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
}
