namespace SignPuddle.API.Data.Repositories
{
    public class SignSearchParameters
    {
        public string? Gloss { get; set; }
        public string? DictionaryId { get; set; }
        public string? Fsw { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
