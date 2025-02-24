using System.ComponentModel.DataAnnotations;

namespace app.entities
{
    public class AuditEntity
    {
        [MaxLength(60)]
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        [MaxLength(60)]
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
