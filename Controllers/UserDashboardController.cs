using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PakProperties.Models;

namespace PakProperties.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager; // Add SignInManager

        public UserDashboardController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }
        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            // return View();
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            if (user == null)
            {
                return NotFound();
            }

            var profile = new UserProfile
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            };

            return View(profile);
        }
    }
}
