namespace SignPuddle.API.Data.Repositories
{
    public class DictionarySearchParameters
    {
        public string? Query { get; set; }
        public string? OwnerId { get; set; }
        public bool? IsPublic { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public void Validate()
        {
            if (Page.HasValue && Page < 1) Page = 1;
            if (PageSize.HasValue && (PageSize < 1 || PageSize > 100)) PageSize = 10;
            if (Query != null) Query = Query.Trim();
            if (OwnerId != null) OwnerId = OwnerId.Trim();
        }
    }
}
