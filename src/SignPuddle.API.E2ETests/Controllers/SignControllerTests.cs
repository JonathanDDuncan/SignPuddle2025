using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SignPuddle.API.E2ETests.Fixtures;
using Xunit;

namespace SignPuddle.API.E2ETests.Controllers
{

    public class SignControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SignControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetSign_ReturnsOk_WhenSignExists()
        {
            // Arrange
            var signId = 1; // Replace with a valid sign ID for testing

            // Act
            var response = await _client.GetAsync($"/api/signs/{signId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateSign_ReturnsCreated_WhenSignIsValid()
        {
            // Arrange
            var newSign = new { Name = "Test Sign", Description = "Test Description" }; // Adjust according to your model

            // Act
            var response = await _client.PostAsJsonAsync("/api/signs", newSign);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSign_ReturnsNoContent_WhenSignExists()
        {
            // Arrange
            var signId = 1; // Replace with a valid sign ID for testing

            // Act
            var response = await _client.DeleteAsync($"/api/signs/{signId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}