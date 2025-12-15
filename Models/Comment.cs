using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace blog.Models // Match your namespace
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;

        public int BlogPostId { get; set; }

        // Navigation properties
        public BlogPost BlogPost { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}