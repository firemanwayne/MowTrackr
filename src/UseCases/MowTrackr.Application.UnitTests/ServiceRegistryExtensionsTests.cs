namespace MowTrackr.Application.UnitTests;

public sealed class ServiceRegistryExtensionsTests
{
    [Fact]
    public void AddApplicationServices_EveryApiServiceInterface_IsRegistered()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices(options => options.BaseAddress = new Uri("https://api.test/"));

        var missing = ApiServiceInterfaces()
            .Where(type => !services.Any(descriptor => descriptor.ServiceType == type))
            .Select(type => type.Name)
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void AddApplicationServices_FindsTheApiServiceInterfaces_SoTheCheckAboveIsNotEmpty()
    {
        Assert.NotEmpty(ApiServiceInterfaces());
    }

    private static IEnumerable<Type> ApiServiceInterfaces()
    {
        return typeof(IAuthApiService)
            .Assembly
            .GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("ApiService", StringComparison.Ordinal));
    }
}
