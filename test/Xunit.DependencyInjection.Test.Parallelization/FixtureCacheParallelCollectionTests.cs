#pragma warning disable CS9113, xUnit1051
using System.Collections.Concurrent;

namespace Xunit.DependencyInjection.Test.Parallelization;

/// <summary>
/// Integration tests that reproduce the AsyncLocal race condition through actual parallel collections.
/// Both collections call SetCollection on AssemblySharedFixtures and verify that the _shared_
/// cache is the same instance across collections, confirming the corruption mechanism.
/// </summary>

[CollectionDefinition("FixtureCacheRace_A")]
public class FixtureCacheRaceCollectionA : ICollectionFixture<TrackedCollectionFixture>;

[CollectionDefinition("FixtureCacheRace_B")]
public class FixtureCacheRaceCollectionB : ICollectionFixture<TrackedCollectionFixture>;

public class TrackedCollectionFixture
{
    public long Id { get; }
    internal static ConcurrentBag<int> CacheHashCodes { get; set; } = new();
    internal static ConcurrentBag<long> ActiveThreadIds { get; set; } = new();

    public TrackedCollectionFixture() => Id = Interlocked.Increment(ref _staticId);

    long _staticId;
}

[Collection("FixtureCacheRace_A")]
public class FixtureCacheRaceTests_A(TrackedCollectionFixture fixture)
{
    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task RecordSharedCache(int _)
    {
        TrackedCollectionFixture.ActiveThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

        for (var i = 0; i < 10; i++)
        {
            var cache = FixtureCache.AssemblySharedFixtures;
            TrackedCollectionFixture.CacheHashCodes.Add(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(cache));
            await Task.Delay(50);
        }
    }
}

[Collection("FixtureCacheRace_B")]
public class FixtureCacheRaceTests_B(TrackedCollectionFixture fixture)
{
    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task RecordSharedCache(int _)
    {
        TrackedCollectionFixture.ActiveThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

        for (var i = 0; i < 10; i++)
        {
            var cache = FixtureCache.AssemblySharedFixtures;
            TrackedCollectionFixture.CacheHashCodes.Add(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(cache));
            await Task.Delay(50);
        }
    }
}

/// <summary>
/// Summary test: AssemblySharedFixtures is a single static instance shared by all parallel collections.
/// This PASSES with the bug (confirms shared instance) and would FAIL if the fix creates per-collection instances.
/// </summary>
public class FixtureCacheRaceSummary
{
    [Fact]
    public void CheckSharedCacheIdentity()
    {
        var hashCodes = TrackedCollectionFixture.CacheHashCodes.Distinct().ToList();
        var threads = TrackedCollectionFixture.ActiveThreadIds.Distinct().ToList();

        TrackedCollectionFixture.CacheHashCodes = new();
        TrackedCollectionFixture.ActiveThreadIds = new();

        Assert.True(threads.Count >= 2,
            $"Collections ran on only {threads.Count} thread. Parallelism not active.");

        Assert.True(hashCodes.Count == 1,
            $"AssemblySharedFixtures is shared across {threads.Count} threads with {hashCodes.Count} distinct instances. " +
            $"If > 1, the fix creates per-collection instances and isolation is working. " +
            $"If == 1, all parallel collections share the same FixtureCache (race condition confirmed).");
    }
}
