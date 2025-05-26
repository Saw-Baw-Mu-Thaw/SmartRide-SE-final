using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRide.Models
{
    [Table("Locations")]
    public class Location
    {
        [Key]
        [ForeignKey(nameof(ride))]
        public int RideId { get; set; }
        [Precision(9,6)]
        public decimal PickupLat { get; set; }
        [Precision(9, 6)]
        public decimal PickupLong { get; set; }
        [Precision(9, 6)]
        public decimal DropoffLat { get; set; }
        [Precision(9, 6)]
        public decimal DropoffLong { get; set; }

        public Ride ride { get; set; }
    }
}
