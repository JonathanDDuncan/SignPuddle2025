using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class Dictionary
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Language { get; set; } = string.Empty; // ISO code
        
        public bool IsPublic { get; set; } = true;
        
        public string? OwnerId { get; set; } // User ID of owner
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}