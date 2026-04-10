using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using HPMS.Modules.Identity.Data;
using HPMS.Scheduling.Data;

namespace HPMS.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    // Helper to get a Database Context inside a test
    protected T GetService<T>() where T : notnull 
        => Factory.Services.CreateScope().ServiceProvider.GetRequiredService<T>();
}