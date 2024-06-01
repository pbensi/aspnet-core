using System.ComponentModel.DataAnnotations;

namespace api.entities
{
    public class AuditEntity
    {
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        [MaxLength(50)]
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
