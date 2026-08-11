using System.Diagnostics.CodeAnalysis;
using Xunit;
using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

[assembly: AssemblyFixture(typeof(FixtureForAssembly))]
namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

[CollectionDefinition(nameof(MixedCollectionFixtures))]
public class MixedCollectionFixtures : ICollectionFixture<FixtureForCollection>;

[Collection(nameof(MixedCollectionFixtures))]
[SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "Injected via DI")]
public class MixedFixturePropertyTest : IClassFixture<FixtureForClass>
{
    public required FixtureForAssembly AssemblyFixture { get; set; }
    public required FixtureForCollection CollectionFixture { get; set; }
    public required FixtureForClass ClassFixture { get; set; }

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
