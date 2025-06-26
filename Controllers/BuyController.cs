using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PakProperties.Data;
using PakProperties.Models;

namespace PakProperties.Controllers
{
    [Authorize]
    public class BuyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public BuyController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: HomeCards

        public async Task<IActionResult> BuyIndex(string city, string customCity, string type, int? minPrice, int? maxPrice, int? bedrooms, int? bathrooms, int? areaSize)
        {
            if (city == "Other" && !string.IsNullOrEmpty(customCity))
                city = customCity;

            var query = _context.Sell.Where(r => r.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(city))
                query = query.Where(p => p.City == city);

            if (!string.IsNullOrEmpty(type) && type != "All")
                query = query.Where(p => p.Type == type);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == bedrooms.Value.ToString());

            if (bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms == bathrooms.Value.ToString());

            if (areaSize.HasValue)
                query = query.Where(p => p.AreaSize == areaSize.Value);

            // ViewBag for filter persistence
            ViewBag.SelectedType = string.IsNullOrEmpty(type) ? "All" : type;
            ViewBag.SelectedCity = city;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Bedrooms = bedrooms;
            ViewBag.Bathrooms = bathrooms;
            ViewBag.AreaSize = areaSize;

            var properties = await query.ToListAsync();
            return View(properties);
        }


        [Authorize]
        public async Task<IActionResult> ViewProperty(int? id)
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
}
}
