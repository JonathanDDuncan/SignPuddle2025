using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class Symbol
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Key { get; set; } = string.Empty; // ISWA symbol key
        
        public string Category { get; set; } = string.Empty; // Symbol category
        public string Group { get; set; } = string.Empty; // Symbol group
        
        public int BaseKey { get; set; } // Base symbol key
        
        public string? SvgPath { get; set; } // SVG path data for rendering
    }
}