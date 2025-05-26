using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartRide.Models;

namespace SmartRide.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Ride> Rides { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notifications> Notifications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
