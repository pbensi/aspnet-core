namespace Web.Host.Models
{
    public class ResponseModel
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
