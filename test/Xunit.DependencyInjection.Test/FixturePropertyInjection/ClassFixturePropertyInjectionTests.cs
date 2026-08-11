using System.Diagnostics.CodeAnalysis;
using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

/// <summary>
/// Base class declares required property; derived classes have no boilerplate constructors.
/// </summary>
[TestCaseOrderer(typeof(TestCaseByMethodNameOrderer))]
public abstract class TestsWithClassFixtureBase : IClassFixture<FixtureForClass>
{
    public required FixtureForClass Fixture { get; set; }
}

[SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "Injected via DI")]
public class ClassFixtureViaRequiredPropertyTest : TestsWithClassFixtureBase
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
