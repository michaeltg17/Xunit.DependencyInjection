namespace Xunit.DependencyInjection.Test.FixturePropertyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IDependency, DependencyClass>()
            .AddScoped<BeforeAfterTest, BeforeAfter.BeforeAfterFixtureInjector>();
}
