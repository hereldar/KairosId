.PHONY: tests benchmarks format

tests:
	dotnet test tests/KairosId.Tests/KairosId.Tests.csproj

benchmarks:
	dotnet run -c Release --project benchmark/KairosId.Benchmarks/KairosId.Benchmarks.csproj

format:
	dotnet csharpier format .

