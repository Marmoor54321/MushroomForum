using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MushroomForum.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; //na razie nie używane
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var posts = _context.Posts
                .Include(p => p.ForumThread)
                .Include(p => p.User);
            return View(await posts.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.ForumThread)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create(int forumThreadId)
        {
            if (forumThreadId <= 0 || !_context.ForumThreads.Any(t => t.ForumThreadId == forumThreadId))
            {
                return NotFound("Nie znaleziono wątku.");
            }

            ViewData["ForumThreadId"] = forumThreadId;
            return View(new Post());
        }

        // POST: Posts/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,ForumThreadId")] Post post, IFormFile mediaFile)
        {
            if (mediaFile == null || mediaFile.Length == 0)
            {
                ModelState.Remove("mediaFile");
            }

            if (!ModelState.IsValid || post.ForumThreadId <= 0 || !_context.ForumThreads.Any(t => t.ForumThreadId == post.ForumThreadId))
            {
                if (post.ForumThreadId <= 0 || !_context.ForumThreads.Any(t => t.ForumThreadId == post.ForumThreadId))
                {
                    ModelState.AddModelError("ForumThreadId", "Nieprawidłowy identyfikator wątku.");
                }
                ViewData["ForumThreadId"] = post.ForumThreadId;
                return View(post);
            }

            post.IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            post.CreatedAt = DateTime.Now;

            if (mediaFile != null && mediaFile.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(mediaFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mediaFile.CopyToAsync(stream);
                }

                post.Media.Add(new Media
                {
                    Url = "/uploads/" + fileName,
                    Type = mediaFile.ContentType.StartsWith("image") ? "image" : "video"
                });
            }

            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }

        // POST: Posts/Like
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                return NotFound("Post nie został znaleziony.");
            }

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.IdentityUserId == userId && pl.PostId == postId);

            if (existingLike != null)
            {
                return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
            }

            var like = new PostLike
            {
                IdentityUserId = userId,
                PostId = postId
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }

        // POST: Posts/Unlike
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                return NotFound("Post nie został znaleziony.");
            }

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.IdentityUserId == userId && pl.PostId == postId);

            if (like == null)
            {
                return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
            }

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }
    }
}