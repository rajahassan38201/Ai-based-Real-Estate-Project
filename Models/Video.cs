using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PakProperties.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required(ErrorMessage = "Please select a video type.")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a description.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;


        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        public bool IsApproved { get; set; } = false;


        public string FilePath { get; set; } = string.Empty;

        [NotMapped] 
        public IFormFile? VideoFile { get; set; } 


        // Foreign Key for User
        public string UserId { get; set; } = string.Empty;
        public virtual Users? User { get; set; }

    }
}