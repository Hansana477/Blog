using blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor injects DbContext via DI
        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog/Index - Lists all posts
        // GET: /Blog/Index - Lists posts with search and pagination
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5; // Posts per page
            var posts = from p in _context.BlogPosts select p;

            // Apply search if provided
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

            var post = await _context.BlogPosts.FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post); // Pass post to view
        }
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Blog/Create - Save new post
        [HttpPost]
        [ValidateAntiForgeryToken] // Security against CSRF
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

        // Helper method for concurrency check
        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }
}
