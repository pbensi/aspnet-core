using api.shared.Utilities;

namespace api.shared.Dto
{
    public class AuditEntityDto
    {
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
