using System.Diagnostics.CodeAnalysis;

namespace Xunit.DependencyInjection;

public class DependencyInjectionContext(IHost host, bool disableParallelization)
{
    private static readonly AsyncLocal<FixtureCache?> AsyncLocalFixtures = new();

    /// <summary>
    /// Per-collection fixture cache via AsyncLocal. Falls back to shared static for assembly fixtures.
    /// </summary>
    internal static FixtureCache Fixtures => AsyncLocalFixtures.Value ?? FixtureCache.AssemblySharedFixtures;

    public IHost Host { get; } = host;

    public IServiceProvider RootServices => Host.Services;

    public bool DisableParallelization { get; } = disableParallelization;
}

/// <summary>
/// Holds fixture instances for resolving <c>[Required]</c> properties.
/// Assembly fixtures are shared via a static field; collection/class are per-runner via AsyncLocal,
/// so that parallel collections get their own isolation for collection/class fixtures while
/// assembly fixtures are visible from all threads.
/// </summary>
public sealed class FixtureCache
{
    /// <summary>Shared across all threads for assembly fixtures — set by assembly runner.</summary>
    internal static readonly FixtureCache AssemblySharedFixtures = new();
    private IDictionary<Type, object>? _assembly;

    IDictionary<Type, object>? _collection;
    IDictionary<Type, object>? _class;

    /// <summary>Always writes to the shared assembly fixtures.</summary>
    internal static void SetAssembly(IDictionary<Type, object>? fixtures) => AssemblySharedFixtures._assembly = fixtures;
    internal void SetCollection(IDictionary<Type, object>? fixtures) => _collection = fixtures;
    internal void SetClass(IDictionary<Type, object>? fixtures) => _class = fixtures;

    /// <summary>
    /// Tries to get a fixture instance by type. Priority: class > collection > assembly.
    /// </summary>
    internal bool TryGet(Type fixtureType, [MaybeNullWhen(false)] out object instance)
    {
        if (_class?.TryGetValue(fixtureType, out instance) == true) return true;
        if (_collection?.TryGetValue(fixtureType, out instance) == true) return true;
        if (AssemblySharedFixtures._assembly?.TryGetValue(fixtureType, out instance) == true) return true;
        instance = default;
        return false;
    }
}

public class DependencyInjectionBuildContext(IHost host, bool disableParallelization) : DependencyInjectionContext(host, disableParallelization)
{
    public bool Disposed { get; set; }
}

public class DependencyInjectionTestContext(
    IHost host,
    bool disableParallelization,
    bool force,
    int maxParallelThreads,
    SemaphoreSlim? parallelSemaphore)
    : DependencyInjectionContext(host, disableParallelization)
{
    public bool ForcedParallelization { get; } = force;

    public int MaxParallelThreads { get; } = maxParallelThreads;

    public SemaphoreSlim? ParallelSemaphore { get; } = parallelSemaphore;
}

public class DependencyInjectionStartupContext(
    IHost? defaultHost,
    ParallelizationMode parallelizationMode,
    IReadOnlyDictionary<IXunitTestClass, DependencyInjectionBuildContext?> contextMap)
{
    public IServiceProvider? DefaultRootServices => defaultHost?.Services;

    public ParallelizationMode ParallelizationMode { get; } = parallelizationMode;

    public IReadOnlyDictionary<IXunitTestClass, DependencyInjectionBuildContext?> ContextMap { get; } = contextMap;

    public SemaphoreSlim? ParallelSemaphore { get; internal set; }

    public int MaxParallelThreads { get; internal set; }
}

public enum ParallelizationMode
{
    None = 0,
    Enhance = 1,
    Force = 2
}
