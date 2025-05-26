using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.E2ETests.Controllers
{
    public class RenderControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RenderControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetRender_ReturnsOk()
        {
            // Arrange
            var requestUri = "/api/render"; // Adjust the endpoint as necessary

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Additional tests for other RenderController endpoints can be added here
    }
}