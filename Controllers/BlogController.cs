using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using blog.Models; // Replace 'blog' with your actual project namespace if different

namespace blog.Controllers // Match your project namespace
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog/Index - Lists posts with search and pagination
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;
            var postsQuery = _context.BlogPosts.Include(p => p.Likes).Include(p => p.Comments); // Eager load for counts

            var posts = postsQuery.Select(p => p);

            // Apply search if provided (case-insensitive)
            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                posts = posts.Where(p => p.Title.ToLower().Contains(lowerSearch) || p.Content.ToLower().Contains(lowerSearch));
            }

            // Order by date descending (newest first)
            posts = posts.OrderByDescending(p => p.PostedDate);

            // Pagination: Skip/Take for current page
            var totalPosts = await posts.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages)); // Clamp page to valid range

            var paginatedPosts = await posts.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Pass data to view
            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(paginatedPosts);
        }

        // GET: /Blog/Details/5 - View single post by ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.BlogPosts
                .Include(p => p.Likes)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserHasLiked = userId != null && post.Likes.Any(l => l.UserId == userId);
            ViewBag.LikeCount = post.Likes.Count;
            ViewBag.CommentCount = post.Comments.Count;

            return View(post);
        }

        // GET: /Blog/Create - Show form (requires admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Blog/Create - Save new post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Content,Author")] BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                blogPost.PostedDate = DateTime.Now; // Set on creation
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to list
            }
            return View(blogPost); // Return form with errors
        }

        // GET: /Blog/Edit/5 - Show edit form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            return View(blogPost);
        }

        // POST: /Blog/Edit/5 - Update post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Author,PostedDate")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // GET: /Blog/Delete/5 - Confirm delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: /Blog/Delete/5 - Perform delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Blog/AddComment - Add a comment (requires login)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Any logged-in user
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Content required" });
            }

            var comment = new Comment
            {
                Content = content,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                BlogPostId = postId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Return partial view for AJAX
            var newComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);
            return PartialView("_CommentPartial", newComment);
        }

        // POST: /Blog/ToggleLike/5 - Like/Unlike post (requires login)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.BlogPostId == postId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Json(new { liked = false, count = await GetLikeCount(postId) });
            }
            else
            {
                var like = new Like { UserId = userId, BlogPostId = postId };
                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return Json(new { liked = true, count = await GetLikeCount(postId) });
            }
        }

        // Helper method for concurrency check
        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }

        // Helper for like count
        private async Task<int> GetLikeCount(int postId)
        {
            return await _context.Likes.CountAsync(l => l.BlogPostId == postId);
        }
    }
}