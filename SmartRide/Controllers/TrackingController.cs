using Microsoft.AspNetCore.Mvc;
using SmartRide.Data;
using SmartRide.Models;

namespace SmartRide.Controllers
{
    public class TrackingController : Controller
    {
        public readonly ApplicationDbContext context;

        public TrackingController(ApplicationDbContext _context)
        {
            context = _context;
        }

        public IActionResult Index(string custName, string phoneNumber, int rideId)
        {
            var model = new CustInfo
            {
                customerName = custName ?? "Unknown",
                phoneNumber = phoneNumber ?? "No phone number available",
                rideId = rideId
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Complete(int rideId)
        {
            var ride = context.Rides.Find(rideId);
            ride.status = "Completed";
            context.Rides.Update(ride);
            context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Arrived(int rideId)
        {
            var ride = context.Rides.Find(rideId);
            ride.DropOffTime = DateTime.Now;
            context.Rides.Update(ride);
            context.SaveChanges();
            return RedirectToAction("Index", "Payment", new { rideId = rideId});
        }
    }
}
