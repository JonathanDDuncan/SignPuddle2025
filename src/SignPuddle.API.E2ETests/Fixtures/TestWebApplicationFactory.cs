using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"), optional: true);
            configBuilder.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // Ensure the same PipeWriter workaround is applied in tests
            services.Configure<MvcOptions>(options =>
            {
                options.SuppressOutputFormatterBuffering = true;
            });
        });

        builder.UseEnvironment("Testing");

        // Add detailed logging to see exceptions
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        builder.ConfigureTestServices(services =>
        {
            // Register test services or replace existing services with test doubles
            // For example:
            // services.RemoveAll<IDbConnection>();
            // services.AddScoped<IDbConnection>(sp => new TestDbConnection());

            // You can also seed test data, configure test databases, etc.
        });
    }

    // The CreateHostBuilder method is no longer needed for .NET 6+ projects
    // as WebApplicationFactory handles the Program.Main directly

    // Optionally add helper methods for your tests
    public HttpClient CreateAuthenticatedClient(string userId = "test-user")
    {
        var client = CreateClient();
        // Add authentication headers or tokens as needed
        // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestToken(userId));
        return client;
    }

    // Helper method to get detailed error information
    public async Task<string> GetDetailedErrorAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        return string.Empty;
    }
}