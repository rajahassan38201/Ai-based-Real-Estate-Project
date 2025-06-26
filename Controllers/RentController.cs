    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PakProperties.Data;
    using PakProperties.Models;
 

    namespace PakProperties.Controllers
    {
        public class RentController : Controller
        {
            private readonly AppDbContext _context;
            private readonly IWebHostEnvironment _env;
            public RentController(AppDbContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }


            [HttpGet]
            public async Task<IActionResult> RentPortal()
            {
                var rents = await _context.Rent.ToListAsync(); // ✅ await the data
                return View(rents); // ✅ pass the actual list
            }


        public async Task<IActionResult> ViewRentProperty(int? id)
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


            [Authorize]
            public IActionResult RentUpload()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> RentUpload(Rent data)
            {
                if (ModelState.IsValid)
                {
                    var allowedExtensions = new List<string> { ".webp", ".png", ".jpg", ".jpeg" };
                    const long maxSize = 5000000; // 5 MB

                    // Validate primary image
                    if (data.PrimaryImage == null)
                    {
                        TempData["ErrorMessage"] = "Primary image is required.";
                        return View(data);
                    }

                    var primaryExt = Path.GetExtension(data.PrimaryImage.FileName).ToLower();
                    var primarySize = data.PrimaryImage.Length;

                    if (!allowedExtensions.Contains(primaryExt))
                    {
                        TempData["ext"] = "Only .webp, .png, .jpg, or .jpeg are allowed for the primary image.";
                        return View(data);
                    }

                    if (primarySize > maxSize)
                    {
                        TempData["size"] = "Primary image size must be less than 5MB.";
                        return View(data);
                    }

                    // Prepare files (only process non-null optional images)
                    var files = new List<(IFormFile File, string FileName, string PropertyName)>
            {
                (data.PrimaryImage, Guid.NewGuid().ToString() + "_" + data.PrimaryImage.FileName, "PrimaryImageUrl")
            };

                    if (data.Image2 != null)
                        files.Add((data.Image2, Guid.NewGuid().ToString() + "_" + data.Image2.FileName, "ImageUrl2"));
                    if (data.Image3 != null)
                        files.Add((data.Image3, Guid.NewGuid().ToString() + "_" + data.Image3.FileName, "ImageUrl3"));
                    if (data.Image4 != null)
                        files.Add((data.Image4, Guid.NewGuid().ToString() + "_" + data.Image4.FileName, "ImageUrl4"));
                    if (data.Image5 != null)
                        files.Add((data.Image5, Guid.NewGuid().ToString() + "_" + data.Image5.FileName, "ImageUrl5"));

                    foreach (var (file, fileName, propertyName) in files)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var size = file.Length;

                        if (!allowedExtensions.Contains(ext))
                        {
                            TempData["ext"] = $"Only .webp, .png, .jpg, or .jpeg are allowed. Invalid file: {file.FileName}";
                            return View(data);
                        }

                        if (size > maxSize)
                        {
                            TempData["size"] = $"Image size must be less than 5MB. Invalid file: {file.FileName}";
                            return View(data);
                        }
                    }

                    string folder = Path.Combine(_env.WebRootPath, "images/sell");

                    // Save the images
                    foreach (var (file, fileName, propertyName) in files)
                    {
                        string filePath = Path.Combine(folder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Assign the saved file name to the corresponding property
                        if (propertyName == "PrimaryImageUrl") data.PrimaryImageUrl = fileName;
                        else if (propertyName == "ImageUrl2") data.ImageUrl2 = fileName;
                        else if (propertyName == "ImageUrl3") data.ImageUrl3 = fileName;
                        else if (propertyName == "ImageUrl4") data.ImageUrl4 = fileName;
                        else if (propertyName == "ImageUrl5") data.ImageUrl5 = fileName;
                    }

                    // Set user ID for the model
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["ErrorMessage"] = "User is not logged in.";
                        return RedirectToAction("Index", "Home");
                    }

                    data.UserId = userId;

                    // Save the model to the database
                    _context.Add(data);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sell listing created successfully!";
                    return RedirectToAction("Index", "Home");
                }

                // Return validation errors if any
                TempData["ErrorMessage"] = "Error creating the sell listing.";
                return View(data);
            }

            [Authorize]
            public async Task<IActionResult> RentIndex(string city, string customCity, string type, int? minPrice, int? maxPrice, int? bedrooms, int? bathrooms, int? areaSize)
            {
                if (city == "Other" && !string.IsNullOrEmpty(customCity))
                {
                    city = customCity;
                }

            var query = _context.Rent.Where(r => r.IsApproved).AsQueryable();

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

                // ViewBag persistence for UI fields
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
        public async Task<IActionResult> Detail(int id, string? returnUrl = null)
        {
            var rent = await _context.Rent.FindAsync(id);
            if (rent == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("PendingPropertyApprovals", "AdminDashboard");
            return View(rent);
        }




        public async Task<IActionResult> Edit(int? id)
            {
                if (id == null || _context.Rent == null)
                {
                    return NotFound();
                }

                var data = await _context.Rent.FindAsync(id);
                if (data == null)
                {
                    return NotFound();
                }
                return View(data);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Rent data)
            {
                if (ModelState.IsValid)
                {
                    var allowedExtensions = new List<string> { ".webp", ".png", ".jpg", ".jpeg" };
                    const long maxSize = 5000000; // 5 MB

                    // Validate primary image
                    if (data.PrimaryImage == null)
                    {
                        TempData["ErrorMessage"] = "Primary image is required.";
                        return View(data);
                    }

                    var primaryExt = Path.GetExtension(data.PrimaryImage.FileName).ToLower();
                    var primarySize = data.PrimaryImage.Length;

                    if (!allowedExtensions.Contains(primaryExt))
                    {
                        TempData["ext"] = "Only .webp, .png, .jpg, or .jpeg are allowed for the primary image.";
                        return View(data);
                    }

                    if (primarySize > maxSize)
                    {
                        TempData["size"] = "Primary image size must be less than 5MB.";
                        return View(data);
                    }

                    // Prepare files (only process non-null optional images)
                    var files = new List<(IFormFile File, string FileName, string PropertyName)>
            {
                (data.PrimaryImage, Guid.NewGuid().ToString() + "_" + data.PrimaryImage.FileName, "PrimaryImageUrl")
            };

                    if (data.Image2 != null)
                        files.Add((data.Image2, Guid.NewGuid().ToString() + "_" + data.Image2.FileName, "ImageUrl2"));
                    if (data.Image3 != null)
                        files.Add((data.Image3, Guid.NewGuid().ToString() + "_" + data.Image3.FileName, "ImageUrl3"));
                    if (data.Image4 != null)
                        files.Add((data.Image4, Guid.NewGuid().ToString() + "_" + data.Image4.FileName, "ImageUrl4"));
                    if (data.Image5 != null)
                        files.Add((data.Image5, Guid.NewGuid().ToString() + "_" + data.Image5.FileName, "ImageUrl5"));

                    foreach (var (file, fileName, propertyName) in files)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var size = file.Length;

                        if (!allowedExtensions.Contains(ext))
                        {
                            TempData["ext"] = $"Only .webp, .png, .jpg, or .jpeg are allowed. Invalid file: {file.FileName}";
                            return View(data);
                        }

                        if (size > maxSize)
                        {
                            TempData["size"] = $"Image size must be less than 5MB. Invalid file: {file.FileName}";
                            return View(data);
                        }
                    }

                    string folder = Path.Combine(_env.WebRootPath, "images/sell");

                    // Save the images
                    foreach (var (file, fileName, propertyName) in files)
                    {
                        string filePath = Path.Combine(folder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Assign the saved file name to the corresponding property
                        if (propertyName == "PrimaryImageUrl") data.PrimaryImageUrl = fileName;
                        else if (propertyName == "ImageUrl2") data.ImageUrl2 = fileName;
                        else if (propertyName == "ImageUrl3") data.ImageUrl3 = fileName;
                        else if (propertyName == "ImageUrl4") data.ImageUrl4 = fileName;
                        else if (propertyName == "ImageUrl5") data.ImageUrl5 = fileName;
                    }

                    // Set user ID for the model
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["ErrorMessage"] = "User is not logged in.";
                        return RedirectToAction("Index", "Home");
                    }

                    data.UserId = userId;

                    // Save the model to the database
                    _context.Update(data);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sell listing created successfully!";
                    return RedirectToAction("Index", "Home");
                }

                // Return validation errors if any
                TempData["ErrorMessage"] = "Error creating the sell listing.";
                return View(data);
            }

            public async Task<IActionResult> Delete(int? id)
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

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
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
                return RedirectToAction(nameof(Index));
            }

        }
    }
