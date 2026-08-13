using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

[CollectionDefinition(nameof(CollectionForPropertyInjection))]
public class CollectionForPropertyInjection : ICollectionFixture<FixtureForCollection>;

/// <summary>
/// Base class declares required property; derived classes have no boilerplate constructors.
/// </summary>
[Collection(nameof(CollectionForPropertyInjection))]
public abstract class CollectionFixtureViaRequiredBase
{
    public required FixtureForCollection Fixture { get; set; }

    internal static FixtureForCollection? _first;
}

[Collection(nameof(CollectionForPropertyInjection))]
public class CollectionFixtureViaRequiredPropertyTest_A : CollectionFixtureViaRequiredBase
{
    [Fact]
    public void CollectionFixtureIsNotNull() => Assert.NotNull(Fixture);

    [Fact]
    public void CollectionFixtureDependencyInjected() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void CollectionFixtureShared_A()
    {
        var previous = Interlocked.CompareExchange(ref _first, Fixture, null);
        Assert.True(previous == null || ReferenceEquals(previous, Fixture));
    }
}

[Collection(nameof(CollectionForPropertyInjection))]
public class CollectionFixtureViaRequiredPropertyTest_B : CollectionFixtureViaRequiredBase
{
    [Fact]
    public void CollectionFixtureDependencyType() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void CollectionFixtureShared_B()
    {
        var previous = Interlocked.CompareExchange(ref _first, Fixture, null);
        Assert.True(previous == null || ReferenceEquals(previous, Fixture));
    }
}
