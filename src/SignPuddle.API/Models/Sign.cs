using System;
using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Models
{
    public class Sign
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public int PuddleSignId { get; set; }
        
         public string? DictionaryId { get; set; }
         public string? PuddleId { get; set; }
   
        public string? Fsw { get; set; } = string.Empty; // Formal SignWriting notation
        
        public string? Gloss { get; set; } // Text translation/meaning

        public Dictionary? Dictionary { get; set; }
        
        public string? SgmlText { get; set; } // Sign text (if part of a sequence)
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}