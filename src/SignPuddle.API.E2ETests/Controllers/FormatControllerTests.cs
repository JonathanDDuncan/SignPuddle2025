using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.E2ETests.Controllers
{
    public class FormatControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public FormatControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetFormats_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/formats");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFormat_ReturnsOk_WhenFormatExists()
        {
            // Arrange
            var formatId = 1;

            // Act
            var response = await _client.GetAsync($"/api/formats/{formatId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateFormat_ReturnsCreated_WhenFormatIsValid()
        {
            // Arrange
            var newFormat = new { Name = "Test Format", Type = "svg" };
            var json = JsonSerializer.Serialize(newFormat);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/formats", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}