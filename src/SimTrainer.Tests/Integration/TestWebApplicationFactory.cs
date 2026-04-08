using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimTrainer.Api.Infrastructure;

namespace SimTrainer.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SimTrainerDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Use in-memory database for tests
            services.AddDbContext<SimTrainerDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            });
        });

        builder.UseEnvironment("Development");
    }
}
