using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentix.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // İşlemi yapan kullanıcı (Null olabilir, örneğin sistem otomatik yaptıysa)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Örn: "Create", "Delete"

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Örn: "Vehicle", "Rental"

        public DateTime Date { get; set; } = DateTime.Now;
    }
}