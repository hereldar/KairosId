using System;
using BenchmarkDotNet.Attributes;
using KairosId;

namespace KairosId.Benchmarks;

[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class IdBenchmarks
{
    private static readonly string GuidString = Guid.NewGuid().ToString();
    private static readonly string UlidString = Ulid.NewUlid().ToString();
    private static readonly string KairosIdString = KairosId.NewKairosId().ToString();

    // Generation
    [Benchmark(Baseline = true)]
    public Guid NewGuid() => Guid.NewGuid();

    [Benchmark]
    public Ulid NewUlid() => Ulid.NewUlid();

    [Benchmark]
    public KairosId NewKairosId() => KairosId.NewKairosId();

    // To String
    [Benchmark]
    public string GuidToString() => Guid.NewGuid().ToString();

    [Benchmark]
    public string UlidToString() => Ulid.NewUlid().ToString();

    [Benchmark]
    public string KairosIdToString() => KairosId.NewKairosId().ToString();

    // Parsing
    [Benchmark]
    public Guid ParseGuid() => Guid.Parse(GuidString);

    [Benchmark]
    public Ulid ParseUlid() => Ulid.Parse(UlidString);

    [Benchmark]
    public KairosId ParseKairosId() => KairosId.Parse(KairosIdString);

    // Formats
    [Benchmark]
    public string KairosIdToBase58() => KairosId.NewKairosId().ToBase58();

    [Benchmark]
    public string KairosIdToBase32() => KairosId.NewKairosId().ToBase32();

    [Benchmark]
    public string KairosIdToHex() => KairosId.NewKairosId().ToHex();
}
