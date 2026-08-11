namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public static class Fixtures
{
    public abstract class Fixture(IDependency dependency)
    {
        public IDependency Dependency { get; } = dependency;
    }

    public class FixtureForAssembly(IDependency dependency) : Fixture(dependency) { }
    public class FixtureForCollection(IDependency dependency) : Fixture(dependency) { }
    public class FixtureForClass(IDependency dependency) : Fixture(dependency) { }
}
