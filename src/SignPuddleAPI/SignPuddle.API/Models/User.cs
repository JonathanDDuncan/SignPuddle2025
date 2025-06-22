using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        
        public bool IsAdmin { get; set; } = false;
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; }
    }
}