using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;
using SmartRide.Data;
using SmartRide.Models;
using System.Reflection;

namespace SmartRide.Controllers
{
    public class BookingController : Controller
    {
        [BindProperty]
        public InputModel Input { get; set; }

        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public BookingController(ApplicationDbContext _context, UserManager<IdentityUser> _userManager)
        {
            context = _context;
            userManager = _userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Booking")]
        public async Task<IActionResult> OnPost(decimal pickupLong,decimal pickupLat,
            decimal dropoffLong, decimal dropoffLat, string vehicleType)
        {
            if(pickupLat == 0 || pickupLong == 0 ||
               dropoffLat == 0 || dropoffLong == 0 ||
               string.IsNullOrEmpty(vehicleType))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View("Index");
            }

            Input = new InputModel
            {
                PickUpLat = pickupLat,
                PickupLong = pickupLong,
                DropoffLat = dropoffLat,
                DropoffLong = dropoffLong,
                VehicleType = vehicleType
            };
            var unavailableDrivers = context.Rides.Where(r => r.status == "InTransit" ||
                                                              r.status == "Waiting")
                                                  .Select(r => r.DriverId)
                                                  .ToList();

            //var drivers = (from user in context.Users
            //              where userManager.GetClaimsAsync(user).Result
            //                            .Any(c => c.Type == "Role" && c.Value == "Driver") &&
            //                    !unavailableDrivers.Contains(user.Id)
            //              select user).ToList();
            var drivers = new List<IdentityUser>();

            foreach(var user in context.Users)
            {
                var claims = await userManager.GetClaimsAsync(user);
                if(claims.Any(c => c.Type == "Role" && c.Value == "Driver")
                    && !unavailableDrivers.Contains(user.Id))
                {
                    drivers.Add(user);
                }
            }

            if(drivers.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No available drivers at the moment.");
                return View("Index");
            }
            
            

            var ride = new Ride
            {
                CustomerId = userManager.GetUserId(User),
                PickupTime = DateTime.Now,
                status = "Waiting",
            };

            context.Rides.Add(ride);
            context.SaveChanges();

            var location = new Location
            {
                ride = ride,
                RideId = ride.RideId,
                PickupLat = pickupLat,
                PickupLong = pickupLong,
                DropoffLat = dropoffLat,
                DropoffLong = dropoffLong,
            };

            context.Locations.Add(location);
            context.SaveChanges();

            IdentityUser selectedDriver = null;
            foreach (var d in drivers)
            {
                var noti = new Notifications
                {
                    Message = $"New ride request",
                    driverId = d.Id,
                    RideId = ride.RideId,
                };
                context.Notifications.Add(noti);
                context.SaveChanges();

                Thread.Sleep(10000); // Simulate waiting for driver to accept the ride

                //noti = context.Notifications.First(n => n.Id == noti.Id);
                context.Entry(noti).Reload();

                if (noti.Accept == true)
                {
                    selectedDriver = d;
                    break;
                }
                else
                {
                    context.Notifications.Remove(noti);
                    context.SaveChanges();
                }
            }
            if(selectedDriver == null)
            {
                ride.status = "Cancelled";
                context.Rides.Update(ride);
                ModelState.AddModelError(string.Empty, "No driver accepted the ride request.");
                return View("Index");
            }

            ride.DriverId = selectedDriver.Id;
            context.Rides.Update(ride);
            context.SaveChanges();

            var pageModel = new DriverInfo
            {
                RideId = ride.RideId,
                Id = selectedDriver.Id,
                Description = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "Description")?.Value ?? "No description available",
                Name = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "Username")?.Value ?? "Unknown",
                VIN = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "VIN")?.Value ?? "No VIN available",
            };

             //return View("Track", pageModel);
            return RedirectToAction("Track", new {rideId = ride.RideId, id = selectedDriver.Id, description = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "Description")?.Value ?? "No description available",
                name = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "Username")?.Value ?? "Unknown",
                VIN = userManager.GetClaimsAsync(selectedDriver).Result
                    .FirstOrDefault(c => c.Type == "VIN")?.Value ?? "No VIN available"
            });
        }

        public IActionResult Track(int rideId, string id, string description, string name, string VIN)
        {
            var model = new DriverInfo
            {
                RideId = rideId,
                Id = id,
                Description = description,
                Name = name,
                VIN = VIN
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Cancel(int rideId)
        {
            var ride = context.Rides.Find(rideId);
            if (ride == null)
            {
                ModelState.AddModelError(string.Empty, "Ride not found.");
                return View("Index");
            }
            ride.status = "Cancelled";
            context.Rides.Update(ride);
            context.SaveChanges();

            return View("Index");
        }

        [HttpPost]
        public IActionResult Accept(int rideId, int notiId)
        {
            var ride = context.Rides.Find(rideId);
            if (ride == null)
            {
                ModelState.AddModelError(string.Empty, "Ride not found.");
                return View();
            }
            var notification = context.Notifications.Find(notiId);
            if (notification != null)
            {
                notification.Accept = true;
                notification.IsRead = true;
                context.Notifications.Update(notification);
                context.SaveChanges();
            }
            var customer = context.Users.Find(ride.CustomerId);
            var customerName = userManager.GetClaimsAsync(customer).Result
                .FirstOrDefault(c => c.Type == "Username")?.Value ?? "Unknown";
            var phoneNumber = customer.PhoneNumber;
            return RedirectToAction("Index", "Tracking", new {custName = customerName, phoneNumber = phoneNumber, rideId = rideId});
        }

        [HttpPost]
        public IActionResult Reject(int rideId, int notiId)
        {
            var ride = context.Rides.Find(rideId);
            if(ride == null)
            {
                ModelState.AddModelError(string.Empty, "Ride not found.");
                return View();
            }
            var notification = context.Notifications.Find(notiId);
            if (notification != null)
            {
                notification.Accept = false;
                notification.IsRead = true;
                context.Notifications.Update(notification);
                context.SaveChanges();
            }
            return View();

        }

        public IActionResult Requests()
        {
            var userId = userManager.GetUserId(User);
            var notifications = context.Notifications
                .Where(n => n.driverId == userId && !n.IsRead)
                .ToList();
            return View(notifications);
        }


    }

    public class InputModel
    {
        public decimal PickUpLat { get; set; }
        public decimal PickupLong { get; set; }
        public decimal DropoffLat { get; set; }
        public decimal DropoffLong { get; set; }
        public string VehicleType { get; set; }
    }
}
