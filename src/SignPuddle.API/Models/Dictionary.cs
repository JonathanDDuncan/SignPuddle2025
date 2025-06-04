using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class Dictionary
    {
        [Key]
        public int Id { get; set; }
        
        public string? PuddleId { get; set; }
        public string? PuddleType { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
               
        public bool IsPublic { get; set; } = true;
        
        public string? OwnerId { get; set; } // User ID of owner
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow; 
    }
}