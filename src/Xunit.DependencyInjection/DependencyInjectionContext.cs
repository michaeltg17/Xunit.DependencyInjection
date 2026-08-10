using System.Diagnostics.CodeAnalysis;

namespace Xunit.DependencyInjection;

public class DependencyInjectionContext(IHost host, bool disableParallelization)
{
    /// <summary>Thread-static accessor for fixture cache, reachable from any runner regardless of context type.</summary>
    [field: ThreadStatic] internal static FixtureCache Fixtures { get => field ??= new(); private set; }

    public IHost Host { get; } = host;

    public IServiceProvider RootServices => Host.Services;

    public bool DisableParallelization { get; } = disableParallelization;
}

/// <summary>
/// Holds references to fixtures from class, collection, and assembly scopes,
/// threaded via ThreadStatic so that <c>[Required]</c> property injection and
/// <c>BeforeAfterTest</c> in the per-test scope can resolve already-created fixtures
/// without crossing DI scope boundaries.
/// </summary>
public sealed class FixtureCache
{
    IDictionary<Type, object>? _assembly;
    IDictionary<Type, object>? _collection;
    IDictionary<Type, object>? _class;

    internal void SetAssembly(IDictionary<Type, object>? fixtures) => _assembly = fixtures;
    internal void SetCollection(IDictionary<Type, object>? fixtures) => _collection = fixtures;
    internal void SetClass(IDictionary<Type, object>? fixtures) => _class = fixtures;

    /// <summary>
    /// Tries to get a fixture instance by type. Priority: class > collection > assembly.
    /// </summary>
    public bool TryGet(Type fixtureType, [MaybeNullWhen(false)] out object instance)
    {
        if (_class?.TryGetValue(fixtureType, out instance) == true) return true;
        if (_collection?.TryGetValue(fixtureType, out instance) == true) return true;
        if (_assembly?.TryGetValue(fixtureType, out instance) == true) return true;
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
