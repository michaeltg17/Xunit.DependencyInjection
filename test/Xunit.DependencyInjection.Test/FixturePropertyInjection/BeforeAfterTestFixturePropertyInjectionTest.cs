using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static Xunit.DependencyInjection.Test.FixturePropertyInjection.Fixtures;

namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

[SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "Set via BeforeAfterTest")]
public class BeforeAfterTestFixturePropertyInjectionTest : IClassFixture<FixtureForClass>, IDisposable
{
    public FixtureForClass? Fixture { get; set; }

    [Fact]
    public void FixtureInjectedInBefore() => Assert.NotNull(Fixture);

    public void Dispose()
    {
        Assert.Null(Fixture);
        GC.SuppressFinalize(this);
    }
}

public class BeforeAfterFixtureInjector : BeforeAfterTest
{
    public override void Before(object? testClassInstance, MethodInfo method)
    {
        if (testClassInstance is BeforeAfterTestFixturePropertyInjectionTest test
            && FixtureCache.TryGet(typeof(FixtureForClass), out var fixture))
            test.Fixture = (FixtureForClass)fixture;
    }

    public override void After(object? testClassInstance, MethodInfo method)
    {
        if (testClassInstance is BeforeAfterTestFixturePropertyInjectionTest test)
            test.Fixture = null;
    }
}
