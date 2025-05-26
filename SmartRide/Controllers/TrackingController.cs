using Microsoft.AspNetCore.Mvc;

namespace SmartRide.Controllers
{
    public class TrackingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
