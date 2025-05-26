using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SmartRide.Models
{
    [Table("Rides")]
    public class Ride
    {
        [Key]
        public int RideId { get; set; }
        [ForeignKey(nameof(Customer))]
        public string? CustomerId { get; set; }
        [ForeignKey(nameof(Driver))]
        public string? DriverId { get; set; }
        [AllowNull]
        public DateTime PickupTime { get; set; }
        [AllowNull]
        public DateTime DropOffTime { get; set; }
        public string status { get; set; } = "Waiting";

        public IdentityUser Customer { get; set; }
        public IdentityUser Driver { get; set; }
    }
}
