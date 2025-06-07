using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    public class AchievementsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public List<AchievementType> AchievedAchievements { get; set; } = new();
        public List<AchievementType> UnachievedAchievements { get; set; } = new();

        public AchievementsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Pobierz wszystkie typy osi¹gniêæ
            var allAchievements = await _context.AchievementTypes.ToListAsync();

            // Pobierz osi¹gniêcia zdobyte przez u¿ytkownika
            var userAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == user.Id)
                .Select(ua => ua.AchievementTypeId)
                .ToListAsync();

            // Podziel na zdobyte / niezdobyte
            AchievedAchievements = allAchievements.Where(a => userAchievementIds.Contains(a.Id)).ToList();
            UnachievedAchievements = allAchievements.Where(a => !userAchievementIds.Contains(a.Id)).ToList();

            return Page();
        }
    }
}
