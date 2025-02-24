namespace app.shared.Dto
{
    public class OffsetQueryDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? Search { get; set; }
    }
}
