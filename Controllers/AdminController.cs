using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Models; // Your namespace

namespace blog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Fetch stats for dashboard
            var totalPosts = await _context.BlogPosts.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalComments = await _context.Comments.CountAsync();
            var totalLikes = await _context.Likes.CountAsync();
            var recentPosts = await _context.BlogPosts.OrderByDescending(p => p.PostedDate).Take(5).ToListAsync();
            var recentComments = await _context.Comments.Include(c => c.User).OrderByDescending(c => c.PostedDate).Take(5).ToListAsync();

            ViewBag.TotalPosts = totalPosts;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalComments = totalComments;
            ViewBag.TotalLikes = totalLikes;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.RecentComments = recentComments;

            return View();
        }

        // Quick action links (redirect to Blog CRUD)
        public IActionResult ManagePosts() => RedirectToAction("Index", "Blog"); // Edit posts
        public IActionResult ManageUsers() => RedirectToAction("Index", "Users"); // If you add Users controller later
    }
}