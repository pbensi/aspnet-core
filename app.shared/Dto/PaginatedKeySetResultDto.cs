namespace app.shared.Dto
{
    public class PaginatedKeySetResultDto<T>
    {
        public List<T> Data { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? Search { get; set; }

        private string? _PreviousCursor;
        public string? PreviousCursor
        {
            get => _PreviousCursor;
            set => _PreviousCursor = SecurityUtils.PublicEncrypt(value);
        }

        private string? _NextCursor;
        public string? NextCursor
        {
            get => _NextCursor;
            set => _NextCursor = SecurityUtils.PublicEncrypt(value);
        }

        public int? PreviousUniqueId { get; set; }
        public bool HasPreviousPage { get; set; }
        public int? NextUniqueId { get; set; }
        public bool HasNextPage { get; set; }
    }
}
