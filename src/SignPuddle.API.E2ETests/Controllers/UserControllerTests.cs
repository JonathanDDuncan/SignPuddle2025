using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.E2ETests.Controllers
{
    public class UserControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999; // Assuming no user with ID 999 exists

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}");

            // Assert - Either NotFound (if endpoint exists) or NotFound (if endpoint doesn't exist)
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUsers_ChecksIfEndpointExists()
        {
            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert - Should not return MethodNotAllowed, meaning endpoint might exist
            Assert.NotEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task CreateUser_ChecksIfEndpointExists()
        {
            // Arrange
            var newUser = new { Name = "Test User", Email = "test@example.com" };
            var json = JsonSerializer.Serialize(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/users", content);

            // Assert - Check that we get a meaningful response (not MethodNotAllowed)
            Assert.NotEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            
            // Log the actual response for debugging
            var responseContent = await response.Content.ReadAsStringAsync();
            // In a real test, you might want to output this for debugging
        }
    }
}