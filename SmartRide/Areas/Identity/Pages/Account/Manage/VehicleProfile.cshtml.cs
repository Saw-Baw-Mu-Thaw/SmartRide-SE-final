using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRide.Areas.Identity.Pages.Account.Manage
{
    public class VehicleProfileModel : PageModel
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public VehicleProfileModel(UserManager<IdentityUser> _userManager,
            SignInManager<IdentityUser> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }

        [BindProperty]
        public VehicleModel Input { get; set; }

        private async Task LoadAsync()
        {
            Input = new VehicleModel
            {
                License = User.FindFirst("License").Value,
                VIN = User.FindFirst("VIN").Value,
                Description = User.FindFirst("Description").Value,
                VehicleType = User.FindFirst("VehicleType").Value
            };
        
        }

        public async Task<IActionResult> OnGet()
        {
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(User.Identity.Name);

                await userManager.RemoveClaimAsync(user, User.FindFirst("VIN"));
                await userManager.RemoveClaimAsync(user, User.FindFirst("License"));
                await userManager.RemoveClaimAsync(user, User.FindFirst("Description"));
                await userManager.RemoveClaimAsync(user, User.FindFirst("VehicleType"));

                await userManager.AddClaimAsync(user, new Claim("VIN", Input.VIN));
                await userManager.AddClaimAsync(user, new Claim("License", Input.License));
                await userManager.AddClaimAsync(user, new Claim("Description", Input.Description));
                await userManager.AddClaimAsync(user, new Claim("VehicleType", Input.VehicleType));

                await signInManager.SignInAsync(user, false);
                return Page();
            }
            return Page();
        }
    }

    public class VehicleModel
    {
        [Required]
        [Display(Name = "VIN")]
        public string VIN { get; set; }

        [Required]
        [Display(Name = "Car Description")]
        public string Description { get; set; } = "";

        [Required]
        [Display(Name = "License")]
        public string License { get; set; } = "";

        [Required]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = "";
    }
}
