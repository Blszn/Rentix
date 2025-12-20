using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rentix.Models.Enums;

namespace Rentix.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        public int Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyPrice { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Available;

        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public virtual ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
    }
}