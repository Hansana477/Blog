using Microsoft.EntityFrameworkCore;

namespace blog.Models
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<BlogPost> BlogPosts { get; set; } // Table for BlogPosts

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
