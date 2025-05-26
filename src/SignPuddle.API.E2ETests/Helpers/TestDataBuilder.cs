using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using System;
using System.Collections.Generic;
using SignPuddle.API.Models;


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
        public async Task Api_IsRunning()
        {
            // Test if the API is running by checking a basic endpoint
            var response = await _client.GetAsync("/");
            
            // Should return some response (even if 404, it means the server is running)
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Swagger_IsAvailable_InDevelopment()
        {
            // Test if Swagger is available
            var response = await _client.GetAsync("/swagger");
            
            // In development, this should redirect or return the swagger page
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.MovedPermanently);
        }
    }
}

namespace SignPuddle.API.E2ETests.Helpers
{
    public static class TestDataBuilder
    {
        public static Sign CreateSign(int id = 1)
        {
            return new Sign
            {
                Id = id
                // Add only the properties that actually exist in your Sign model
            };
        }

        public static List<Sign> CreateSignList(int count)
        {
            var signs = new List<Sign>();
            for (int i = 1; i <= count; i++)
            {
                signs.Add(CreateSign(id: i));
            }
            return signs;
        }

        public static User CreateUser(string id = "1", string username = "testuser", string email = "testuser@example.com")
        {
            return new User
            {
                Id = id,
                Username = username,
                Email = email
            };
        }

        public static List<User> CreateUserList(int count)
        {
            var users = new List<User>();
            for (int i = 1; i <= count; i++)
            {
                users.Add(CreateUser(id: i.ToString(), username: $"testuser{i}", email: $"testuser{i}@example.com"));
            }
            return users;
        }
    }
}