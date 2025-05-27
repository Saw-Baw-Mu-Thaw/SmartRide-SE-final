using Microsoft.AspNetCore.Mvc;
using SmartRide.Data;

namespace SmartRide.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext context;

        public PaymentController(ApplicationDbContext _context)
        {
            context = _context;
        }

        public IActionResult Index(int rideId)
        {
            var model = context.Payments.First(p => p.RideId == rideId);

            return View(model);
        }

        [HttpPost]
        public IActionResult Paid()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
