using System.ComponentModel.DataAnnotations;

namespace Rentix.Models.Enums
{
    public enum VehicleStatus
    {
        [Display(Name = "Müsait")]
        Available = 0,
        [Display(Name = "Kirada")]
        Rented = 1,
        [Display(Name = "Bakımda")]
        Maintenance = 2
    }
}