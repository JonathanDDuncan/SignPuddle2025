namespace SignPuddle.API.Data.Repositories
{
    public class SignSearchParameters
    {
        public string? Gloss { get; set; }
        public string? DictionaryId { get; set; }
        public string? Fsw { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public void Validate()
        {
            if (Page.HasValue && Page < 1) Page = 1;
            if (PageSize.HasValue && (PageSize < 1 || PageSize > 100)) PageSize = 10;
            if (Gloss != null) Gloss = Gloss.Trim();
            if (DictionaryId != null) DictionaryId = DictionaryId.Trim();
            if (Fsw != null) Fsw = Fsw.Trim();
        }
    }
}
