namespace app.shared.Dto
{
    public class KeysetQueryDto
    {
        private string? _PreviousCursor;
        public string? PreviousCursor
        {
            get => SecurityUtils.PublicDecrypt(_PreviousCursor);
            set => _PreviousCursor = value;
        }

        private string? _NextCursor;
        public string? NextCursor
        {
            get => SecurityUtils.PublicDecrypt(_NextCursor);
            set => _NextCursor = value;
        }

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
