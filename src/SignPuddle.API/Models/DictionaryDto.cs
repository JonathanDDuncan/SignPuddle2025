namespace SignPuddle.API.Models
{
    public class DictionaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string? OwnerId { get; set; }
    }
}
