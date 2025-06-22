using System;
using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class Sign
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public int PuddleSignId { get; set; }
        
         public string? DictionaryId { get; set; }
         public string? PuddleId { get; set; }
   
        public string? Fsw { get; set; } = string.Empty; // Formal SignWriting notation

        public List<string> Gloss { get; set; } = new List<string>(); // Text translation/meaning

        public Dictionary? Dictionary { get; set; }
        
        public string? Description { get; set; } 
        
        public DateTime Created { get; set; } 
        public DateTime Updated { get; set; } 
        
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
 
    }
}