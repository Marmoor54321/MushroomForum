using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.Services;
using MushroomForum.Services.Service;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MushroomForum.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AchievementService _achievementService;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AchievementService achievementService)
        {
            _context = context;
            _userManager = userManager;
            _achievementService = achievementService;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _achievementService.GrantAchievementIfNotExistsAsync(userId, "FirstPost");

            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }

        // POST: Posts/Like
        // POST: Posts/Like
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null) return NotFound();

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.IdentityUserId == userId && pl.PostId == postId);

            if (existingLike == null)
            {
                // Dodaj nowy PostLike (obecny stan)
                _context.PostLikes.Add(new PostLike
                {
                    IdentityUserId = userId,
                    PostId = postId
                });

                // Sprawdź, czy już kiedyś był lajk – jeśli nie, to przyznaj exp
                bool hasLikedBefore = await _context.LikeHistories
                    .AnyAsync(lh => lh.LikerId == userId && lh.PostId == postId);

                if (!hasLikedBefore)
                {
                    // Dodaj wpis do LikeHistory
                    _context.LikeHistories.Add(new LikeHistory
                    {
                        LikerId = userId,
                        PostId = postId,
                        LikedAt = DateTime.UtcNow
                    });

                    // Przyznaj 10 exp autorowi posta
                    if (!string.IsNullOrEmpty(post.IdentityUserId))
                    {
                        var levelUpService = HttpContext.RequestServices.GetRequiredService<LevelUpService>();
                        await levelUpService.GiveExperienceAsync(post.IdentityUserId, 10);
                    }
                }

                await _context.SaveChangesAsync();
            }

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
            if (post == null) return NotFound();

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.IdentityUserId == userId && pl.PostId == postId);

            if (like != null)
            {
                _context.PostLikes.Remove(like);
                await _context.SaveChangesAsync();
            }

            // LikeHistory NIE jest ruszany – zostaje jako dowód, że exp był przyznany

            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }


        // GET: Posts/Reply/5
        public IActionResult Reply(int postId)
        {
            var parentPost = _context.Posts
                .Include(p => p.ForumThread)
                .FirstOrDefault(p => p.PostId == postId);

            if (parentPost == null)
                return NotFound();

            var reply = new Post
            {
                ForumThreadId = parentPost.ForumThreadId,
                ParentPostId = parentPost.PostId
            };

            ViewData["ParentPost"] = parentPost;
            return View(reply);
        }

        // POST: Posts/Reply
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([Bind("Description,ForumThreadId,ParentPostId")] Post post)
        {
            if (!ModelState.IsValid)
            {
                var parentPost = _context.Posts
                    .Include(p => p.ForumThread)
                    .FirstOrDefault(p => p.PostId == post.ParentPostId);
                ViewData["ParentPost"] = parentPost;
                return View(post);
            }

            post.IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            post.CreatedAt = DateTime.Now;

            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumThreads", new { id = post.ForumThreadId });
        }
    }
}