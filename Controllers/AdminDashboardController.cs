using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PakProperties.Data;
using PakProperties.Models;
using PakProperties.ViewModels;

using System.Security.Claims;

namespace PakProperties.Controllers
{
    // Apply authorization at the controller level
    [Authorize(Roles = "admin")]
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Users> _userManager;


        public AdminDashboardController(AppDbContext context, IWebHostEnvironment env, UserManager<Users> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult AdminDashboard()
        {
            // Get all user profiles
            var userProfiles = _context.Users.Select(u => new UserProfile
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName
            }).ToList();


            var sellCategoryCounts = new Dictionary<string, int>
            {
                ["House"] = _context.Sell.Count(p => p.Type == "House"),
                ["Plot"] = _context.Sell.Count(p => p.Type == "Plot"),
                ["Flat"] = _context.Sell.Count(p => p.Type == "Flat"),
                ["Commercial"] = _context.Sell.Count(p => p.Type == "Commercial"),
            };

            // Category-wise Rent Count (example)
            var rentCategoryCounts = new Dictionary<string, int>
            {
                ["House"] = _context.Rent.Count(p => p.Type == "House"),
                ["Flat"] = _context.Rent.Count(p => p.Type == "Flat")
            };

            ViewBag.SellCategories = sellCategoryCounts;
            ViewBag.RentCategories = rentCategoryCounts;

            // Count Rent and Sell properties
            int rentCount = _context.Rent.Count();
            int sellCount = _context.Sell.Count();

            // Pass counts to ViewBag
            ViewBag.RentCount = rentCount;
            ViewBag.SellCount = sellCount;




            var topSellCities = _context.Sell
    .GroupBy(p => p.City)
    .Select(g => new { City = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .Take(5)
    .ToList();

            var topRentCities = _context.Rent
                .GroupBy(p => p.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            ViewBag.TopSellCities = topSellCities;
            ViewBag.TopRentCities = topRentCities;


            // Return both userProfiles and ViewBag data
            return View(userProfiles);
        }


        public async Task<IActionResult> AllProperties()
        {
            var properties = await _context.Sell.ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Sell
                .FirstOrDefaultAsync(s => s.Id == id);

            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sell == null)
            {
                return NotFound();
            }

            var data = await _context.Sell
                .FirstOrDefaultAsync(m => m.Id == id);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sell == null)
            {
                return Problem("Entity set 'AppDbContext.Sell'  is null.");
            }
            var data = await _context.Sell.FindAsync(id);
            if (data != null)
            {
                _context.Sell.Remove(data);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllProperties));
        }

        public async Task<IActionResult> RentProperties()
        {
            var properties = await _context.Rent.ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> RentDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Rent
                .FirstOrDefaultAsync(s => s.Id == id);

            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        public async Task<IActionResult> RentDelete(int? id)
        {
            if (id == null || _context.Rent == null)
            {
                return NotFound();
            }

            var data = await _context.Rent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        [HttpPost, ActionName("RentDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RentDeleteConfirmed(int id)
        {
            if (_context.Rent == null)
            {
                return Problem("Entity set 'AppDbContext.Rent'  is null.");
            }
            var data = await _context.Rent.FindAsync(id);
            if (data != null)
            {
                _context.Rent.Remove(data);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(RentProperties));
        }

        public async Task<IActionResult> AllVideos()
        {
            var Videos = await _context.Video.ToListAsync();
            return View(Videos);
        }

        public async Task<IActionResult> DetailVideo(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Video
                .Include(v => v.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        public async Task<IActionResult> DeleteVideo(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Video
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteVideoConfirmed")] // unique name

        public async Task<IActionResult> DeleteConfirmedVideo(int id)
        {
            var video = await _context.Video.FindAsync(id);

            if (video == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(video.FilePath))
            {
                var filePath = Path.Combine("wwwroot", video.FilePath.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        // Try to release any locks before deleting
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            // Just opening to release any lock, then closing
                        }

                        System.IO.File.Delete(filePath);
                    }
                    catch (IOException ex)
                    {
                        TempData["ErrorMessage"] = "File is in use and cannot be deleted right now.";
                        return RedirectToAction(nameof(AllVideos));
                    }
                }
            }

            _context.Video.Remove(video);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Video deleted successfully!";
            return RedirectToAction(nameof(AllVideos));
        }


        [HttpPost]
        [Route("AdminDashboard/AssignAdminRole/{id}")]
        public async Task<IActionResult> AssignAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["OpenTab"] = "admins"; // 👈 this flag tells the view to open admin tab
            }
            return RedirectToAction("Index", "Users");
        }

        public async Task<IActionResult> PendingPropertyApprovals(int? id, string? returnUrl)
        {
            var pendingSells = await _context.Sell
                .Where(p => !p.IsApproved)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var pendingRents = await _context.Rent
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            var pendingVideos = await _context.Video
               .Where(r => !r.IsApproved)
               .ToListAsync();

            var viewModel = new PendingApprovalsViewModel
            {
                PendingSells = pendingSells,
                PendingRents = pendingRents,
                PendingVideos = pendingVideos
            };

            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl)
        ? returnUrl
        : Request.Headers["Referer"].ToString(); // fallback if returnUrl not passed

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ApproveSell(int id, string? tab)
        {
            var property = await _context.Sell.FindAsync(id);
            if (property != null)
            {
                property.IsApproved = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }


        [HttpPost]
        public async Task<IActionResult> RejectSell(int id, string? tab)
        {
            var property = await _context.Sell.FindAsync(id);
            if (property != null)
            {
                _context.Sell.Remove(property); // or flag if you want to preserve
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }



        [HttpPost]
        public async Task<IActionResult> ApproveRent(int id, string? tab)
        {
            var rent = await _context.Rent.FindAsync(id);
            if (rent != null)
            {
                rent.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }

        [HttpPost]
        public async Task<IActionResult> RejectRent(int id, string? tab)
        {
            var rent = await _context.Rent.FindAsync(id);
            if (rent != null)
            {
                _context.Rent.Remove(rent);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }


        /// for video approval

        [HttpPost]
        public async Task<IActionResult> ApproveVideo(int id, string? tab)
        {
            var video = await _context.Video.FindAsync(id);

            if (video == null)
            {
                // Optional: log error
                TempData["Error"] = "Video not found.";
                return RedirectToAction("PendingPropertyApprovals", new { tab });
            }

            // Try setting explicitly
            video.IsApproved = true;

            // Confirm it's being tracked
            _context.Video.Update(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Video approved successfully.";
            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }


        [HttpPost]
        public async Task<IActionResult> RejectVideo(int id, string? tab)
        {
            var video = await _context.Video.FindAsync(id);
            if (video != null)
            {
                _context.Video.Remove(video);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PendingPropertyApprovals", new { tab });
        }


    }
}