namespace Xunit.DependencyInjection.Test.FixturePropertyInjection.BeforeAfter;

/// <summary>
/// Injects fixture via BeforeAfterTest hook.
/// </summary>
public class BeforeAfterFixtureTest : IClassFixture<FixtureForPropertyInjection>, IDisposable
{
    public FixtureForPropertyInjection? Fixture { get; set; }

    [Fact]
    public void FixtureInjectedInBefore() => Assert.NotNull(Fixture);

    public void Dispose() => Assert.Null(Fixture);
}

public class BeforeAfterFixtureInjector : BeforeAfterTest
{
    public override void Before(object? testClassInstance, System.Reflection.MethodInfo method)
    {
        if (testClassInstance is BeforeAfterFixtureTest test
            && Fixtures.TryGet(typeof(FixtureForPropertyInjection), out var fixture))
            test.Fixture = (FixtureForPropertyInjection)fixture;
    }

    public override void After(object? testClassInstance, System.Reflection.MethodInfo method)
    {
        if (testClassInstance is BeforeAfterFixtureTest test)
            test.Fixture = null;
    }
}
