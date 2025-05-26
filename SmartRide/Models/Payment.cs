using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRide.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int Total { get; set; }

        [ForeignKey(nameof(ride))]
        public int RideId { get; set; }

        public Ride ride { get; set; }
    }
}
