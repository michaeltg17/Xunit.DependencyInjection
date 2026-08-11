using System.Diagnostics.CodeAnalysis;
using Xunit;
using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

[assembly: AssemblyFixture(typeof(FixtureForMixed))]
namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

[CollectionDefinition(nameof(SameTypeCollectionFixtures))]
public class SameTypeCollectionFixtures : ICollectionFixture<FixtureForMixed>;

[Collection(nameof(SameTypeCollectionFixtures))]
[SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "Injected via DI")]
public class MixedSameTypeFixturePropertyTests : IClassFixture<FixtureForMixed>
{
    public required FixtureForMixed AssemblyFixture { get; set; }
    public required FixtureForMixed CollectionFixture { get; set; }
    public required FixtureForMixed ClassFixture { get; set; }

    [Fact]
    public void PriorityClassWins_AllAreSameInstance()
    {
        Assert.Same(AssemblyFixture, ClassFixture);
        Assert.Same(CollectionFixture, ClassFixture);
        Assert.Same(AssemblyFixture, CollectionFixture);
    }

    [Fact]
    public void ClassDependencyInjected() => Assert.IsType<DependencyClass>(ClassFixture.Dependency);
}