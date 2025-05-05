using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MushroomForum.Controllers
{
    public class ForumThreadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumThreadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ForumThreads
        public async Task<IActionResult> Index(int? pageNumber, string sortOrder, int? categoryId, int? pageSize)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "newest" : sortOrder;

            var threadsQuery = _context.ForumThreads
                .Include(f => f.User)
                .Include(f => f.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                threadsQuery = threadsQuery.Where(t => t.CategoryId == categoryId);
            }

            threadsQuery = sortOrder switch
            {
                "oldest" => threadsQuery.OrderBy(t => t.CreatedAt),
                _ => threadsQuery.OrderByDescending(t => t.CreatedAt)
            };

            int totalThreads = await threadsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalThreads / (double)currentPageSize);
            var threads = await threadsQuery
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            var viewModel = new ForumThreadsIndexViewModel
            {
                Threads = threads,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SortOrder = sortOrder,
                PageNumber = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages,
                TotalThreads = totalThreads
            };

            return View(viewModel);
        }

        // GET: ForumThreads/Details/5
        public async Task<IActionResult> Details(int? id, int pageNumber = 1)
        {
            if (id == null) return NotFound();

            var thread = await _context.ForumThreads
                .Include(t => t.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.ForumThreadId == id);

            if (thread == null) return NotFound();

            int pageSize = 10;
            var posts = await _context.Posts
                .Where(p => p.ForumThreadId == id)
                .Include(p => p.User)
                .Include(p => p.Media)
                .OrderBy(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalPosts = await _context.Posts.CountAsync(p => p.ForumThreadId == id);
            int totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            var viewModel = new ThreadDetailsViewModel
            {
                Thread = thread,
                Posts = posts,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                TotalPosts = totalPosts
            };

            return View(viewModel);
        }

        // GET: ForumThreads/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: ForumThreads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ForumThreadId,Title,CategoryId,Description")] ForumThread forumThread)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                forumThread.IdentityUserId = userId;
                forumThread.CreatedAt = DateTime.Now;

                _context.Add(forumThread);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", forumThread.CategoryId);
            return View(forumThread);
        }
    }
}