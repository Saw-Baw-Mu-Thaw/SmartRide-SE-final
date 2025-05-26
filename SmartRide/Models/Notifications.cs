using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRide.Models
{
    public class Notifications
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; } = "";
        [ForeignKey(nameof(Driver))]
        public string driverId { get; set; } = "";
        public bool IsRead { get; set; } = false;
        public bool Accept { get; set; } = false;
        [ForeignKey(nameof(ride))]
        public int RideId { get; set; }

        public IdentityUser Driver { get; set; }
        public Ride ride { get; set; }
    }
}
