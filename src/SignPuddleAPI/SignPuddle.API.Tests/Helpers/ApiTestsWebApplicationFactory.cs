using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SignPuddle.API.Data; // For ApplicationDbContext
using System.Linq;

namespace SignPuddle.API.Tests.Helpers;

public class ApiTestsWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ensure the environment is set to "Testing".
        // This is crucial to prevent Program.cs from attempting to register Cosmos DB
        // if its logic is triggered by the factory.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Attempt to remove any existing ApplicationDbContext registration.
            // This guards against Program.cs (or other startup logic) having added a DbContext.
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            // Also remove direct ApplicationDbContext registrations if any.
            services.RemoveAll<ApplicationDbContext>();

            // Add ApplicationDbContext using an InMemory database for testing.
            // Using a unique database name per factory instance/test run can help avoid state leakage between tests.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryAppDb-{this.GetType().FullName}-{System.Guid.NewGuid()}");
            });

            // Optionally, add other test-specific service overrides here.
            // For example, mocking external dependencies.
        });
    }
}
