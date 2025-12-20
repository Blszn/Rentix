using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rentix.Models.Enums;

namespace Rentix.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        // İlişkiler (Foreign Keys)
        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Identity User Id'si string'dir

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public RentalStatus RentalStatus { get; set; } = RentalStatus.Active;
    }
}