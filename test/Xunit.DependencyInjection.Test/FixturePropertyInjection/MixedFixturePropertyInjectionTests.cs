using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.DependencyInjection.Test.FixturePropertyInjection;

[assembly: AssemblyFixture(typeof(FixtureForAssemblyMixed))]
namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

/// <summary>
/// Fixture types declared for mixed scope injection tests.
/// Each is a unique type so FixtureCache.TryGet resolves by exact type.
/// </summary>
public class FixtureForAssemblyMixed(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

public class FixtureForCollectionMixed(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

public class FixtureForClassMixed(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

[CollectionDefinition(nameof(MixedCollectionFixtures))]
public class MixedCollectionFixtures : ICollectionFixture<FixtureForCollectionMixed>;

[Collection(nameof(MixedCollectionFixtures))]
[SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "Injected via DI")]
public class MixedFixturePropertyTest : IClassFixture<FixtureForClassMixed>
{
    public required FixtureForAssemblyMixed AssemblyFixture { get; set; }
    public required FixtureForCollectionMixed CollectionFixture { get; set; }
    public required FixtureForClassMixed ClassFixture { get; set; }

    [Fact]
    public void AssemblyFixtureResolved() => Assert.NotNull(AssemblyFixture);

    [Fact]
    public void CollectionFixtureResolved() => Assert.NotNull(CollectionFixture);

    [Fact]
    public void ClassFixtureResolved() => Assert.NotNull(ClassFixture);

    [Fact]
    public void AssemblyDependencyInjected() => Assert.IsType<DependencyClass>(AssemblyFixture.Dependency);

    [Fact]
    public void CollectionDependencyInjected() => Assert.IsType<DependencyClass>(CollectionFixture.Dependency);

    [Fact]
    public void ClassDependencyInjected() => Assert.IsType<DependencyClass>(ClassFixture.Dependency);

    [Fact]
    public void AssemblyAndCollectionAreDifferentInstances()
        => Assert.NotSame(AssemblyFixture.Dependency, CollectionFixture.Dependency);

    [Fact]
    public void AssemblyAndClassAreDifferentInstances()
        => Assert.NotSame(AssemblyFixture.Dependency, ClassFixture.Dependency);

    [Fact]
    public void CollectionAndClassAreDifferentInstances()
        => Assert.NotSame(CollectionFixture.Dependency, ClassFixture.Dependency);
}
