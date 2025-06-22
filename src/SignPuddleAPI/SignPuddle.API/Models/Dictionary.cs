using System.ComponentModel.DataAnnotations;
using System;

namespace SignPuddle.API.Models
{    public class Dictionary
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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