using System.Reflection;

namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public class BeforeAfterTestFixturePropertyInjectionTest : IClassFixture<FixtureForClass>, IDisposable
{
    public FixtureForClass? Fixture { get; set; }

    [Fact]
    public void FixtureInjectedInBefore() => Assert.NotNull(Fixture);

    public void Dispose() => Assert.Null(Fixture);
}

public class BeforeAfterFixtureInjector : BeforeAfterTest
{
    public override void Before(object? testClassInstance, MethodInfo method)
    {
        if (testClassInstance is BeforeAfterTestFixturePropertyInjectionTest test
            && Fixtures.TryGet(typeof(FixtureForClass), out var fixture))
            test.Fixture = (FixtureForClass)fixture;
    }

    public override void After(object? testClassInstance, MethodInfo method)
    {
        if (testClassInstance is BeforeAfterTestFixturePropertyInjectionTest test)
            test.Fixture = null;
    }
}
