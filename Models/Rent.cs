    using Microsoft.EntityFrameworkCore;
    using System;

    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
using PakProperties.ViewModels;

namespace PakProperties.Models
    {
        public class Rent
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Type { get; set; } = string.Empty;

            [Required]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            [Required]
            public string City { get; set; } = string.Empty;

            [Required]
            public string Address { get; set; } = string.Empty;

            [Required]
            public string Bedrooms { get; set; } = string.Empty;

            [Required]
            public string Bathrooms { get; set; } = string.Empty;

            [Required]
            public int AreaSize { get; set; }

            [Required]
            public int Price { get; set; }
        public bool IsApproved { get; set; } = false;

        // Primary image URL (e.g., for thumbnails or main display)
        //public string ImageUrl { get; set; } = string.Empty;

        // Collection of image URLs for multiple images
        //[NotMapped]
        //public List<string>? ImageUrls { get; set; } = new List<string>();

        // Form files for handling image uploads
        //[NotMapped]
        //public List<IFormFile>? Images { get; set; }

        public string PrimaryImageUrl { get; set; } = string.Empty;

            [NotMapped]
            [Required]
            public IFormFile? PrimaryImage { get; set; }

            public string ImageUrl2 { get; set; } = string.Empty;

            [NotMapped]
            public IFormFile? Image2 { get; set; }

            public string ImageUrl3 { get; set; } = string.Empty;

            [NotMapped]
            public IFormFile? Image3 { get; set; }

            public string ImageUrl4 { get; set; } = string.Empty;

            [NotMapped]
            public IFormFile? Image4 { get; set; }

            public string ImageUrl5 { get; set; } = string.Empty;

            [NotMapped]
            public IFormFile? Image5 { get; set; }

            [Required]
            [RegularExpression(@"^[a-zA-Z\s]+$")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Phone]
            [StringLength(11, MinimumLength = 11)]
            [RegularExpression(@"^\d{11}$")]
            public string MobileNo { get; set; } = string.Empty;

            [DataType(DataType.DateTime)]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [DataType(DataType.DateTime)]
            public DateTime? UpdatedAt { get; set; }

            // Foreign Key for the User
            public string UserId { get; set; } = string.Empty;

            // Navigation Property for the User
            [ForeignKey("UserId")]
            public virtual Users? User { get; set; }
        }
    }

