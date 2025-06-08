using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MushroomForum.Areas.Identity.Pages.Account
{
    public class QuestsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public QuestsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<DailyQuestType> TodayQuests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Nie znaleziono u¿ytkownika.");
            }

            var dayOfWeek = DateTime.UtcNow.DayOfWeek;

            TodayQuests = await _context.DailyQuestTypes
                .Where(q => q.DayOfWeek == dayOfWeek) 
                .ToListAsync();


            return Page();
        }
    }
}
