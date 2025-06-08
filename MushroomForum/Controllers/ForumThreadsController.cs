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
        public async Task<IActionResult> Index(int? pageNumber, string sortOrder, int? categoryId, int? pageSize, string searchTerm)
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

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                threadsQuery = threadsQuery.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.User != null && t.User.UserName.Contains(searchTerm))
                );
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
                TotalThreads = totalThreads,
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        // GET: ForumThreads/Details/5
        public async Task<IActionResult> Details(int? id, int pageNumber = 1, int? pageSize = null, string sortOrder = "newest", int? categoryId = null, string searchTerm = null)
        {
            if (id == null) return NotFound();

            var thread = await _context.ForumThreads
                .Include(t => t.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.ForumThreadId == id);

            if (thread == null) return NotFound();

            int postPageSize = pageSize ?? 10;
            var posts = await _context.Posts
                .Where(p => p.ForumThreadId == id && p.ParentPostId == null)
                .Include(p => p.User)
                .Include(p => p.Media)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.User)
                .OrderBy(p => p.CreatedAt)
                .Skip((pageNumber - 1) * postPageSize)
                .Take(postPageSize)
                .ToListAsync();

            int totalPosts = await _context.Posts.CountAsync(p => p.ForumThreadId == id);
            int totalPages = (int)Math.Ceiling(totalPosts / (double)postPageSize);
            int threadLikeCount = await _context.ThreadLikes.CountAsync(tl => tl.ForumThreadId == id);
            var postIds = posts.Select(p => p.PostId).ToList();
            var postLikeCounts = await _context.PostLikes
                .Where(pl => postIds.Contains(pl.PostId))
                .GroupBy(pl => pl.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToListAsync();

            var postLikeCountDict = postLikeCounts.ToDictionary(x => x.PostId, x => x.Count);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool threadLikedByCurrentUser = false;
            HashSet<int> likedPostIds = new();

            if (User.Identity.IsAuthenticated)
            {
                threadLikedByCurrentUser = await _context.ThreadLikes.AnyAsync(tl => tl.ForumThreadId == id && tl.IdentityUserId == userId);
                likedPostIds = _context.PostLikes
                    .Where(pl => pl.IdentityUserId == userId && postIds.Contains(pl.PostId))
                    .Select(pl => pl.PostId)
                    .ToHashSet();
            }

            var viewModel = new ThreadDetailsViewModel
            {
                Thread = thread,
                Posts = posts,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                TotalPosts = totalPosts,
                ThreadLikeCount = threadLikeCount,
                PostLikeCounts = postLikeCountDict,
                LikedPostIds = likedPostIds,
                ThreadLikedByCurrentUser = threadLikedByCurrentUser
            };

            ViewBag.IndexPageNumber = 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;

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

        // POST: ForumThreads/Like
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int threadId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var thread = await _context.ForumThreads.FirstOrDefaultAsync(t => t.ForumThreadId == threadId);

            if (thread == null)
            {
                return NotFound("Wątek nie został znaleziony.");
            }

            var existingLike = await _context.ThreadLikes
                .FirstOrDefaultAsync(tl => tl.IdentityUserId == userId && tl.ForumThreadId == threadId);

            if (existingLike != null)
            {
                return RedirectToAction("Details", new { id = threadId });
            }

            var like = new ThreadLike
            {
                IdentityUserId = userId,
                ForumThreadId = threadId
            };

            _context.ThreadLikes.Add(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = threadId });
        }

        // POST: ForumThreads/Unlike
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlike(int threadId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var thread = await _context.ForumThreads.FirstOrDefaultAsync(t => t.ForumThreadId == threadId);

            if (thread == null)
            {
                return NotFound("Wątek nie został znaleziony.");
            }

            var like = await _context.ThreadLikes
                .FirstOrDefaultAsync(tl => tl.IdentityUserId == userId && tl.ForumThreadId == threadId);

            if (like == null)
            {
                return RedirectToAction("Details", new { id = threadId });
            }

            _context.ThreadLikes.Remove(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = threadId });
        }
    }
}