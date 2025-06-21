namespace SignPuddle.API.Data.Repositories
{
    public class DictionarySearchParameters
    {
        public string? Query { get; set; }
        public string? OwnerId { get; set; }
        public bool? IsPublic { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
