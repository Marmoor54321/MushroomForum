using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MushroomForum.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuizController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return View(quizzes);
        }


        // GET: /Quiz/Take/1
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            return View("Take", quiz);
        }

        // POST: /Quiz/Take/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(int id, int[] selectedAnswers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            int score = 0;
            var correctAnswerIds = quiz.Questions
                .SelectMany(q => q.Answers)
                .Where(a => a.CzyPoprawna)
                .Select(a => a.Id)
                .ToHashSet();

            foreach (var answerId in selectedAnswers)
            {
                if (correctAnswerIds.Contains(answerId))
                    score++;
            }

            var userId = _userManager.GetUserId(User);
            var userExp = await _context.UserExperiences.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userExp == null)
            {
                userExp = new UserExperience { UserId = userId, Doswiadczenie = 0 };
                _context.UserExperiences.Add(userExp);
            }
            userExp.Doswiadczenie += score;
            await _context.SaveChangesAsync();

            ViewBag.Score = score;
            ViewBag.TotalQuestions = quiz.Questions.Count;
            ViewBag.QuizId = quiz.Id;
            ViewBag.CurrentPoints = userExp.Doswiadczenie;  // <-- dodaj tę linię!

            return View("Result");
        }

        public async Task<IActionResult> Results(int quizId, int score, int totalQuestions)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int currentPoints = 0;

            if (userId != null)
            {
                var userExp = await _context.UserExperiences
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (userExp != null)
                {
                    currentPoints = userExp.Doswiadczenie;  // Użyj odpowiedniej właściwości
                }
            }

            ViewBag.Score = score;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.QuizId = quizId;
            ViewBag.CurrentPoints = currentPoints;

            return View("Result");
        }
        public async Task<IActionResult> Ranking()
        {
            // Pobierz użytkowników wraz z punktami i ich nazwami
            var ranking = await _context.UserExperiences
                .OrderByDescending(u => u.Doswiadczenie)
                .Include(u => u.User)  // jeśli UserExperience ma nawigację do IdentityUser
                .ToListAsync();
            var users = await _context.Users.ToListAsync();

            var rankingWithNames = ranking.Select(u => new RankingViewModel
            {
                UserName = users.FirstOrDefault(user => user.Id == u.UserId)?.UserName ?? "Nieznany",
                Points = u.Doswiadczenie
            }).ToList();

            return View(rankingWithNames);
        }
        public async Task<IActionResult> YourRanking()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var ranking = await _context.UserExperiences
                .OrderByDescending(u => u.Doswiadczenie)
                .Include(u => u.User)
                .ToListAsync();

            var rankingWithNames = ranking.Select(u => new RankingViewModel
            {
                UserName = u.User?.UserName ?? "Nieznany",
                Points = u.Doswiadczenie,
                UserId = u.UserId  // musisz dodać UserId do RankingViewModel, jeśli go nie masz
            }).ToList();

            int position = rankingWithNames.FindIndex(r => r.UserId == userId) + 1;

            if (position == 0) position = -1;

            var yourRanking = new YourRankingViewModel
            {
                Position = position,
                Points = rankingWithNames.FirstOrDefault(r => r.UserId == userId)?.Points ?? 0
            };

            var model = new CombinedRankingViewModel
            {
                YourRanking = yourRanking,
                FullRanking = rankingWithNames
            };

            return View(model);
        }


    }
}
