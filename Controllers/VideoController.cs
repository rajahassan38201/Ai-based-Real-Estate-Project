using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PakProperties.Data;
using PakProperties.Models;
using System.IO;
using System.Security.Claims;

namespace PakProperties.Controllers
{
    public class VideoController : Controller
    {
        private readonly AppDbContext _dbContext;

        public VideoController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        public IActionResult UploadVideo()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadVideo(Video model)
        {
            if (ModelState.IsValid && model.VideoFile != null)
            {
                // Check if user is authenticated
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                // Get logged-in user's ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                model.UserId = userId;

                // Validate file extension
                var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv",".mpeg-4" };
                var fileExtension = Path.GetExtension(model.VideoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("VideoFile", "Invalid video format. Only MP4, AVI, MOV, and MKV are allowed.");
                    return View(model);
                }

                // Validate file size (limit: 50 MB)
                if (model.VideoFile.Length > 110 * 1024 * 1024)
                {
                    ModelState.AddModelError("VideoFile", "The video file size must not exceed 50 MB.");
                    return View(model);
                }

                // Define upload folder path
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.VideoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                try
                {
                    // Save file to server
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.VideoFile.CopyToAsync(stream);
                    }

                    // Set file path for database
                    model.FilePath = $"/uploads/{fileName}";

                    // Save video details to database
                    _dbContext.Video.Add(model);
                    await _dbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Video uploaded successfully!";
                    return RedirectToAction("VideoIndex");
                }
                catch (Exception ex)
                {
                    // Log error and show message
                    Console.WriteLine($"Error uploading video: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the video details.");
                    return View(model);
                }
            }

            // If model state is invalid, return form with errors
            return View(model);
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VideoIndex(string? location, string? type)
        {
            var query = _dbContext.Video
                .Include(v => v.User)
                .Where(v => v.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(v => v.Location == location);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(v => v.Type == type);
            }

            var filteredVideos = await query.ToListAsync();
            return View(filteredVideos);
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            //var userVideos = await _dbContext.Video
            //    .Where(s => s.UserId == currentUserId)
            //    .ToListAsync();

            var query = _dbContext.Video.Include(v => v.User).Where(r => r.IsApproved).AsQueryable();


            return View(query);
        }


        [Authorize]
        public async Task<IActionResult> Detail(int id, string? returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _dbContext.Video
                .Include(v => v.User) 
                .FirstOrDefaultAsync(s => s.Id == id);

            if (video == null)
            {
                return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("PendingPropertyApprovals", "AdminDashboard");


            return View(video);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _dbContext.Video.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Video model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var video = await _dbContext.Video.FindAsync(id);

                    if (video == null)
                    {
                        return NotFound();
                    }

                    // Update fields
                    video.Type = model.Type;
                    video.Location = model.Location;
                    video.Description = model.Description;

                    // Check if a new video file was uploaded
                    if (model.VideoFile != null)
                    {
                        // Define the upload folder path
                        var uploadsFolder = Path.Combine("wwwroot/uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Create a unique file name
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.VideoFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save the file
                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.VideoFile.CopyToAsync(stream);
                        }

                        // Delete old video file if exists
                        if (!string.IsNullOrEmpty(video.FilePath))
                        {
                            var oldFilePath = Path.Combine("wwwroot", video.FilePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        video.FilePath = $"/uploads/{fileName}";
                    }

                    _dbContext.Update(video);
                    await _dbContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Video updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the video.");
                    Console.WriteLine(ex);
                }
            }

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _dbContext.Video
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var video = await _dbContext.Video.FindAsync(id);

            if (video == null)
            {
                return NotFound();
            }

            // Delete the video file from the server
            if (!string.IsNullOrEmpty(video.FilePath))
            {
                var filePath = Path.Combine("wwwroot", video.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Remove the video record from the database
            _dbContext.Video.Remove(video);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Video deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


    }
}
        