namespace app.shared.Dto
{
    public class PaginatedOffsetResultDto<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? Search { get; set; }
    }
}
