using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using blog.Models;

namespace blog.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // BlogPost config
            builder.Entity<BlogPost>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(200);
            });

            // Comment config
            builder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(1000);
                entity.HasOne(c => c.BlogPost).WithMany(p => p.Comments).HasForeignKey(c => c.BlogPostId);
                entity.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
            });

            // Like config (composite key for uniqueness)
            builder.Entity<Like>(entity =>
            {
                entity.HasKey(l => new { l.UserId, l.BlogPostId }); // No duplicates
                entity.HasOne(l => l.BlogPost).WithMany(p => p.Likes).HasForeignKey(l => l.BlogPostId);
                entity.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId);
            });
        }
    }
}