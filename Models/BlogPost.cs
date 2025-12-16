using System.ComponentModel.DataAnnotations;

namespace blog.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        public string? Author { get; set; }

        public string? ImagePath { get; set; } // New

        // Navigation (from earlier)
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}