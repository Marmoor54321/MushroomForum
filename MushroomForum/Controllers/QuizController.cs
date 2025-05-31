using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using System.Linq;
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

            // Liczymy punkty za poprawne odpowiedzi
            int score = 0;

            // selectedAnswers zawiera Id odpowiedzi wybranych przez użytkownika
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

            // Dodaj punkty do UserExperience
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

            return View("Result");
        }
    }
}
