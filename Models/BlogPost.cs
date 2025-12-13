using System.ComponentModel.DataAnnotations;

namespace blog.Models
{
    public class BlogPost
    {
        public int Id { get; set; } // Primary key, auto-generated

        [Required] // Ensures title is not empty
        [StringLength(200)] // Max length
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.Date)] // Formats as date
        public DateTime PostedDate { get; set; } = DateTime.Now;

        // Optional: Add author later
        public string? Author { get; set; }
    }
}
