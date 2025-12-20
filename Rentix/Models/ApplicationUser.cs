using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations; // Bunu eklemeyi unutma

namespace Rentix.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Mevcut kodların varsa kalsın, şunları ekle:

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
    }
}