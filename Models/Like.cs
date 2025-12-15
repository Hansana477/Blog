using Microsoft.AspNetCore.Identity;

namespace blog.Models
{
    public class Like
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int BlogPostId { get; set; }

        // Navigation properties
        public IdentityUser User { get; set; } = null!;
        public BlogPost BlogPost { get; set; } = null!;
    }
}