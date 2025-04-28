namespace app.shared.Dto.Account
{
    public class PermissionCheckDto
    {
        public bool HasPermission { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
