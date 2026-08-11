namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public static class Fixtures
{
    public class FixtureForAssembly(IDependency dependency)
    {
        public IDependency Dependency { get; } = dependency;
    }

    public class FixtureForCollection(IDependency dependency)
    {
        public IDependency Dependency { get; } = dependency;
    }

    public class FixtureForClass(IDependency dependency)
    {
        public IDependency Dependency { get; } = dependency;
    }
}
