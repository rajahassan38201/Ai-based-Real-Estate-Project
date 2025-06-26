using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PakProperties.Data;
using PakProperties.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PakProperties.Controllers
{
    public class SellController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SellController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //var properties = await _context.Sell.ToListAsync();
            //return View(properties);

            // Get the current user's ID from the claims
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(); // If the user is not logged in, return unauthorized
            }

            // Query the database for properties input by the current user
            var userProperties = await _context.Sell
                .Where(s => s.UserId == currentUserId)
                .ToListAsync();

            return View(userProperties);


        }

        [Authorize]
        public async Task<IActionResult> Detail(int id, string? returnUrl = null)
        {
            var sell = await _context.Sell.FindAsync(id);
            if (sell == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("PendingPropertyApprovals", "AdminDashboard");
            return View(sell);
        }







        // GET: Create Sell Form
        [Authorize]
        public IActionResult SellIndex()
        {
            return View();
        }

        // POST: Create Sell Listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellIndex(Sell data)
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sell == null)
            {
                return NotFound();
            }

            var data = await _context.Sell.FindAsync(id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sell data)
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

        // POST: HomeCards/Delete/5
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
            return RedirectToAction(nameof(Index));
        }

    }

}



        //public async Task<IActionResult> SellIndex(Sell model, List<IFormFile> images)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Ensure the upload directory exists
        //        string uploadFolder = Path.Combine(_env.WebRootPath, "images/sell");
        //        if (!Directory.Exists(uploadFolder))
        //        {
        //            Directory.CreateDirectory(uploadFolder);
        //        }

        //        // Allowed file extensions and max size
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //        long maxSize = 5 * 1024 * 1024; // 5 MB

        //        // List to store successfully uploaded image URLs
        //        List<string> imagePaths = new List<string>();

        //        if (images != null && images.Any())
        //        {
        //            foreach (var image in images)
        //            {
        //                if (image.Length > 0)
        //                {
        //                    // Validate file extension
        //                    var fileExtension = Path.GetExtension(image.FileName).ToLower();
        //                    if (!allowedExtensions.Contains(fileExtension))
        //                    {
        //                        ModelState.AddModelError("", $"File {image.FileName} has an invalid extension. Allowed: .jpg, .jpeg, .png, .gif");
        //                        continue;
        //                    }

        //                    // Validate file size
        //                    //if (image.Length > maxSize)
        //                    //{
        //                    //    ModelState.AddModelError("", $"File {image.FileName} exceeds the 5 MB limit.");
        //                    //    continue;
        //                    //}

        //                    // Generate unique file name
        //                    string fileName = Guid.NewGuid().ToString() + fileExtension;
        //                    string filePath = Path.Combine(uploadFolder, fileName);

        //                    try
        //                    {
        //                        // Save the image
        //                        using (var stream = new FileStream(filePath, FileMode.Create))
        //                        {
        //                            await image.CopyToAsync(stream);
        //                        }

        //                        // Add the relative path to the list
        //                        string relativePath = $"/images/sell/{fileName}";
        //                        imagePaths.Add(relativePath);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ModelState.AddModelError("", $"Error saving file {image.FileName}: {ex.Message}");
        //                        continue;
        //                    }
        //                }
        //            }

        //            // Assign the first image as the primary image if any images were uploaded
        //            if (imagePaths.Any())
        //            {
        //                model.ImageUrl = imagePaths.First();
        //                model.ImageUrls = imagePaths;
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "No valid images were uploaded.");
        //                return View(model);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Please upload at least one image.");
        //            return View(model);
        //        }

        //        // Set user ID for the model
        //        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            TempData["ErrorMessage"] = "User is not logged in.";
        //            return RedirectToAction("Index", "Home");
        //        }

        //        model.UserId = userId;

        //        // Save the model to the database
        //        _context.Add(model);
        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = "Sell listing created successfully!";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    // Return validation errors if any
        //    TempData["ErrorMessage"] = "Error creating the sell listing.";
        //    return View(model);
        //}
