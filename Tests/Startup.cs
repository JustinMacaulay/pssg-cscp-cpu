public class Startup
{
    /// <summary>
    /// Register dependencies needed for xunit tests
    /// NOTE to register dependencies used by making calls from HttpClient, use CustomWebApplicationFactory
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Gov.Cscp.Victims.Public.Startup> ()
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddAutoMapper();

        services.AddHandlers();

        services.AddTransient<FakeHandlers>();
        services.AddTransient<IFakeRepository, FakeRepository>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<InvoiceHandlers>();
            cfg.RegisterServicesFromAssemblyContaining<FakeHandlers>();
        });

        // add dynamics database adapter
        services.AddDatabase(configuration);
        services.AddTransient<SeedDatabase>();
    }
}
