namespace app.shared.Dto
{
    public class KeysetQueryDto
    {
        public string? PreviousCursor { get; set; }
        public string? NextCursor { get; set; }
        public int PageSize { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? Search { get; set; }
        public int? PreviousUniqueId { get; set; }
        public bool HasPreviousPage { get; set; }
        public int? NextUniqueId { get; set; }
        public bool HasNextPage { get; set; }
    }
}
