using Xunit;
using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

[assembly: AssemblyFixture(typeof(FixtureForAssembly))]

namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

/// <summary>
/// Assembly-level fixture with required property injection.
/// </summary>
[TestCaseOrderer(typeof(TestCaseByMethodNameOrderer))]
public abstract class AssemblyFixtureViaRequiredBase
{
    public required FixtureForAssembly Fixture { get; set; }

    internal static FixtureForAssembly? _first;
}

public class AssemblyFixtureViaRequiredPropertyTest_A : AssemblyFixtureViaRequiredBase
{
    [Fact]
    public void FixtureIsNotNull() => Assert.NotNull(Fixture);

    [Fact]
    public void FixtureDependencyInjected() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void FixtureSharedInstance_A()
    {
        var previous = Interlocked.CompareExchange(ref _first, Fixture, null);
        Assert.True(previous == null || ReferenceEquals(previous, Fixture));
    }
}

public class AssemblyFixtureViaRequiredPropertyTest_B : AssemblyFixtureViaRequiredBase
{
    [Fact]
    public void FixtureDependencyType() => Assert.IsType<DependencyClass>(Fixture.Dependency);

    [Fact]
    public void FixtureSharedInstance_B()
    {
        var previous = Interlocked.CompareExchange(ref _first, Fixture, null);
        Assert.True(previous == null || ReferenceEquals(previous, Fixture));
    }
}
