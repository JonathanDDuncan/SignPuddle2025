using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Data;
using SignPuddle.API.E2ETests.Fixtures;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace SignPuddle.API.E2ETests.Fixtures
{
    public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        // Test database name - will be created and deleted for each test run
        private readonly string _testDatabaseName = $"SignPuddle-Test-{Guid.NewGuid()}";
        
        // CosmosDB Emulator connection string
        private const string EmulatorConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"), optional: true);
                configBuilder.AddEnvironmentVariables();

                // Override with test configuration
                configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:CosmosDb"] = EmulatorConnectionString,
                    ["CosmosDb:DatabaseName"] = _testDatabaseName
                });
            });

            builder.ConfigureServices(services =>
            {
                // Ensure the same PipeWriter workaround is applied in tests
                services.AddControllers(options =>
                {
                    options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
                    };
                    options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(jsonOptions));
                });
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
                // Remove the production CosmosDB registration
                services.RemoveAll<CosmosClient>();
                services.RemoveAll<ApplicationDbContext>();

                // Retrieve the connection string from configuration via service provider
                var sp = services.BuildServiceProvider();
                var config = sp.GetRequiredService<IConfiguration>();
                var cosmosConnectionString = config["ConnectionStrings:CosmosDb"];
                var databaseName = config["CosmosDb:DatabaseName"];

                // Register test CosmosDB client
                services.AddSingleton(_ =>
                {
                    var cosmosClientOptions = new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        SerializerOptions = new CosmosSerializationOptions
                        {
                            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                        }
                    };

                    return new CosmosClient(cosmosConnectionString, cosmosClientOptions);
                });

                // Register test DbContext
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseCosmos(
                        connectionString: cosmosConnectionString,
                        databaseName: databaseName
                    );
                });
            });
        }

        // Clean up test database after tests
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Delete the test database
                    var client = new CosmosClient(EmulatorConnectionString);
                    client.GetDatabase(_testDatabaseName).DeleteAsync().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up test database: {ex.Message}");
                }
            }
            
            base.Dispose(disposing);
        }

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
}