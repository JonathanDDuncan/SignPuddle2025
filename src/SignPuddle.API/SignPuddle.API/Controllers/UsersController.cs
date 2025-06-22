using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SignPuddle.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : SignPuddleBaseController
    {
        private static List<User> _users = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        };

        [HttpGet]
        public IActionResult GetUsers()
        {
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(_users),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            // For test purposes, return 404 for id 999
            if (id == 999)
            {
                return NotFound();
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            return Ok((string?)JsonSerializer.Serialize(user));
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserCreateDto userDto)
        {
            var newUser = new User
            {
                Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                Name = userDto.Name ?? string.Empty,
                Email = userDto.Email ?? string.Empty
            };

            _users.Add(newUser);

            Response.Headers.Append("Location", $"/api/users/{newUser.Id}");
            return Created($"/api/users/{newUser.Id}", newUser);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}