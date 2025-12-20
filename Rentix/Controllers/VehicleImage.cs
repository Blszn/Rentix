using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentix.Models
{
    public class VehicleImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty; // Resmin dosya yolu (örn: /img/cars/bmw1.jpg)

        // Hangi araca ait?
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}