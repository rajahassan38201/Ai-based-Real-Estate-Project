using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PakProperties.Models;
using Microsoft.EntityFrameworkCore;
using PakProperties.Data;

namespace PakProperties.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Sell
                .OrderByDescending(s => s.Id)
                .Where(s => s.IsApproved == true)
                .Take(6)
                .ToListAsync();

            return View(data);
        }
    }
}
