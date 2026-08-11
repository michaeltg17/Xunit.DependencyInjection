using Xunit.DependencyInjection.Test.FixturePropertyInjection;

[assembly: Xunit.AssemblyFixture(typeof(FixtureForAssemblyPropertyInjection))]
[assembly: Xunit.AssemblyFixture(typeof(FixtureForAssemblyClassMixed))]
namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public class FixtureForAssemblyPropertyInjection(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

/// <summary>
/// Assembly-level fixture with required property injection.
/// </summary>
[TestCaseOrderer(typeof(TestCaseByMethodNameOrderer))]
public abstract class AssemblyFixtureViaRequiredBase
{
    public required FixtureForAssemblyPropertyInjection Fixture { get; set; }

    internal static FixtureForAssemblyPropertyInjection? _first;
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

/// <summary>
/// Mixed: assembly + class fixtures together to verify scope boundaries.
/// </summary>
public class FixtureForAssemblyClassMixed(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

public class FixtureForClassMixed(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}

public class MixedAssemblyClassFixtureTest : IClassFixture<FixtureForClassMixed>
{
    public required FixtureForAssemblyClassMixed AssemblyFixture { get; set; }
    public required FixtureForClassMixed ClassFixture { get; set; }

    [Fact]
    public void AssemblyFixtureResolved() => Assert.NotNull(AssemblyFixture);

    [Fact]
    public void ClassFixtureResolved() => Assert.NotNull(ClassFixture);

    [Fact]
    public void AssemblyDependencyInjected() => Assert.IsType<DependencyClass>(AssemblyFixture.Dependency);

    [Fact]
    public void ClassDependencyInjected() => Assert.IsType<DependencyClass>(ClassFixture.Dependency);

    [Fact]
    public void AssemblyAndClassAreDifferentInstances()
        => Assert.NotSame(AssemblyFixture.Dependency, ClassFixture.Dependency);
}
