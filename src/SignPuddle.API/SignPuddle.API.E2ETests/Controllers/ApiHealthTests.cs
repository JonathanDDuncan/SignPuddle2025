using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignPuddle.API.E2ETests.Fixtures;
using Xunit;

namespace SignPuddle.API.E2ETests.Controllers
{
    public class ApiHealthTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiHealthTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsOk()
        {
            // Arrange
            var client = _client;

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsExpectedContentType()
        {
            // Arrange
            var client = _client;

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsHealthyStatus()
        {
            // Arrange
            var client = _client;

            // Act
            var response = await client.GetAsync("/health");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Healthy", content);
        }
    }
}