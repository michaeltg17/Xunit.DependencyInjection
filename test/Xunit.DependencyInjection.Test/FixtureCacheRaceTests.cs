#pragma warning disable xUnit1031 // Test methods should not use blocking task operations
#pragma warning disable xUnit1051 // Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken

using System.Collections.Concurrent;
using System.Diagnostics;

namespace Xunit.DependencyInjection.Test;

/// <summary>
/// Tests that reproduce the AsyncLocal race condition in FixtureCache when parallel collections
/// call SetCollection/SetClass on the shared AssemblySharedFixtures instance.
/// </summary>
public class FixtureCacheRaceTests
{
    /// <summary>
    /// Demonstrates that all threads resolve to the SAME FixtureCache instance (AssemblySharedFixtures)
    /// because AsyncLocalFixtures.Value is never assigned. This is the root cause of the race condition.
    /// </summary>
    [Fact]
    public void FixtureCache_AllThreadsShareSameInstance()
    {
        var main = FixtureCache.AssemblySharedFixtures;
        var results = new ConcurrentBag<FixtureCache>();

        Parallel.Invoke(
            () => results.Add(FixtureCache.AssemblySharedFixtures),
            () => results.Add(FixtureCache.AssemblySharedFixtures),
            () => results.Add(FixtureCache.AssemblySharedFixtures),
            () => results.Add(FixtureCache.AssemblySharedFixtures)
        );

        foreach (var instance in results)
            Assert.Same(main, instance);
    }

    /// <summary>
    /// Demonstrates the deterministic corruption mechanism: SetCollection() on the shared instance
    /// overwrites _collection for ALL callers, causing fixture leakage between parallel collections.
    /// </summary>
    [Fact]
    public void SetCollection_CorruptsSharedInstance()
    {
        var cache = FixtureCache.AssemblySharedFixtures;
        var dictA = new Dictionary<Type, object> { [typeof(long)] = 100L };
        var dictB = new Dictionary<Type, object> { [typeof(long)] = 200L };

        cache.SetCollection(dictA);
        Assert.True(cache.TryGet(typeof(long), out var v1));
        Assert.Equal(100L, v1);

        cache.SetCollection(dictB);

        Assert.True(cache.TryGet(typeof(long), out var v2));
        Assert.Equal(200L, v2);
        Assert.NotEqual(v1, v2);

        cache.SetCollection(null);
    }

    /// <summary>
    /// Stress test: multiple parallel tasks simulate collection runners calling SetCollection
    /// and TryGet on the shared FixtureCache. With the bug, at least one task sees another task's fixture.
    /// </summary>
    [Fact]
    public void ParallelSetCollection_RaceConditionDetected()
    {
        var corruptions = new ConcurrentBag<(string caller, long saw)>();
        var dictA = new Dictionary<Type, object> { [typeof(long)] = 100L };
        var dictB = new Dictionary<Type, object> { [typeof(long)] = 200L };

        var taskA = Task.Run(() => RunCollectionTask(corruptions, "A", dictA));
        var taskB = Task.Run(() => RunCollectionTask(corruptions, "B", dictB));

        Task.WaitAll(taskA, taskB);

        Assert.True(corruptions.Count > 0,
            $"Race condition NOT detected after stress test. {corruptions.Count} corruptions recorded.");
    }

    /// <summary>
    /// Stress test: concurrent readers and writers on the assembly fixture.
    /// Demonstrates the thread-safety risk of non-volatile _assembly field.
    /// </summary>
    [Fact]
    public void ConcurrentAssemblyFixtureAccess()
    {
        var fixtureA = new Dictionary<Type, object> { [typeof(long)] = 1L };
        var fixtureB = new Dictionary<Type, object> { [typeof(long)] = 2L };
        var running = new ManualResetEventSlim(false);

        var readerTasks = new Task[8];
        for (var t = 0; t < 8; t++)
        {
            readerTasks[t] = Task.Run(() =>
            {
                running.Wait();
                for (var i = 0; i < 10_000; i++)
                {
                    FixtureCache.AssemblySharedFixtures.TryGet(typeof(long), out _);
                }
            });
        }

        var writerTask = Task.Run(() =>
        {
            running.Wait();
            for (var i = 0; i < 5_000; i++)
            {
                FixtureCache.SetAssembly((i % 2 == 0) ? fixtureA : fixtureB);
                GC.KeepAlive(FixtureCache.AssemblySharedFixtures);
            }
        });

        Thread.Sleep(10);
        running.Set();

        Task.WaitAll(readerTasks.Concat(new[] { writerTask }).ToArray());
        FixtureCache.SetAssembly(null);
    }

    static async Task RunCollectionTask(
        ConcurrentBag<(string caller, long saw)> corruptions,
        string name,
        Dictionary<Type, object> fixtures)
    {
        var expected = (long)fixtures[typeof(long)];

        for (var i = 0; i < 500; i++)
        {
            FixtureCache.AssemblySharedFixtures.SetCollection(fixtures);
            await Task.Delay(1);

            if (FixtureCache.AssemblySharedFixtures.TryGet(typeof(long), out var v))
            {
                var seen = (long)v;
                if (seen != expected)
                    corruptions.Add((name, seen));
            }
        }
    }
}
