.PHONY: tests benchmarks

tests:
	dotnet test tests/KairosID.Tests/KairosID.Tests.csproj

benchmarks:
	dotnet run -c Release --project benchmark/KairosID.Benchmarks/KairosID.Benchmarks.csproj
