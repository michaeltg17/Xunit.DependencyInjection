namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public class FixtureForPropertyInjection(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

/// <summary>
/// Base class declares required property; derived classes have no boilerplate constructors.
/// </summary>
public abstract class TestsWithRequiredFixtureBase : IClassFixture<FixtureForPropertyInjection>
{
    public required FixtureForPropertyInjection Fixture { get; set; }
}

public class ClassFixtureViaRequiredPropertyTest : TestsWithRequiredFixtureBase
{
    [Fact]
    public void FixtureIsNotNull() => Assert.NotNull(Fixture);

    [Fact]
    public void FixtureDependencyIsInjected() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void FixtureStatePersistedAcrossTests_1()
    {
        Assert.Equal(0, Fixture.Dependency.Value);
        Fixture.Dependency.Value = 9999;
    }

    [Fact]
    public void FixtureStatePersistedAcrossTests_2() => Assert.Equal(9999, Fixture.Dependency.Value);
}

/// <summary>
/// Collection-level fixture with required property injection.
/// </summary>
[CollectionDefinition(nameof(CollectionForPropertyInjection))]
public class CollectionForPropertyInjection : ICollectionFixture<FixtureForPropertyInjection>;

[Collection(nameof(CollectionForPropertyInjection))]
public abstract class CollectionFixtureViaRequiredBase
{
    public required FixtureForPropertyInjection Fixture { get; set; }
}

[Collection(nameof(CollectionForPropertyInjection))]
public class CollectionFixtureViaRequiredPropertyTest_A : CollectionFixtureViaRequiredBase
{
    [Fact]
    public void CollectionFixtureIsNotNull() => Assert.NotNull(Fixture);

    [Fact]
    public void CollectionFixtureDependencyInjected() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void CollectionFixtureState_A()
    {
        Assert.Equal(0, Fixture.Dependency.Value);
        Fixture.Dependency.Value = 7777;
    }
}

[Collection(nameof(CollectionForPropertyInjection))]
public class CollectionFixtureViaRequiredPropertyTest_B : CollectionFixtureViaRequiredBase
{
    [Fact]
    public void CollectionFixtureState_B() => Assert.Equal(7777, Fixture.Dependency.Value);
}

/// <summary>
/// Both class and collection fixtures via required properties.
/// </summary>
public class FixtureForClassPropertyInjection2(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

public class FixtureForCollectionPropertyInjection2(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

[CollectionDefinition(nameof(MixedCollectionFixtures))]
public class MixedCollectionFixtures : ICollectionFixture<FixtureForCollectionPropertyInjection2>;

[Collection(nameof(MixedCollectionFixtures))]
public abstract class MixedFixtureBase : IClassFixture<FixtureForClassPropertyInjection2>
{
    public required FixtureForClassPropertyInjection2 ClassFixture { get; set; }
    public required FixtureForCollectionPropertyInjection2 CollectionFixture { get; set; }
}

[Collection(nameof(MixedCollectionFixtures))]
public class MixedFixturePropertyTest : MixedFixtureBase
{
    [Fact]
    public void ClassFixtureResolved() => Assert.NotNull(ClassFixture);

    [Fact]
    public void CollectionFixtureResolved() => Assert.NotNull(CollectionFixture);

    [Fact]
    public void ClassFixtureDependencyInjected() => Assert.IsType<DependencyClass>(ClassFixture.Dependency);

    [Fact]
    public void CollectionFixtureDependencyInjected() => Assert.IsType<DependencyClass>(CollectionFixture.Dependency);

    [Fact]
    public void ClassAndCollectionAreDifferentInstances()
        => Assert.NotSame(ClassFixture.Dependency, CollectionFixture.Dependency);
}
