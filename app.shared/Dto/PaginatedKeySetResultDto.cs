namespace app.shared.Dto
{
    public class PaginatedKeySetResultDto<T>
    {
        public List<T> Data { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? Search { get; set; }
        public string? PreviousCursor { get; set; }
        public string? NextCursor { get; set; }
        public int? PreviousUniqueId { get; set; }
        public bool HasPreviousPage { get; set; }
        public int? NextUniqueId { get; set; }
        public bool HasNextPage { get; set; }
    }
}
