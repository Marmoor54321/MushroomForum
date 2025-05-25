using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MushroomForum.Data; // lub inny namespace z ApplicationDbContext
using MushroomForum.Models; // lub inny namespace z UserFriend

namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class FriendsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FriendsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<IdentityUser> Friends { get; set; } = new();

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var friendIds = await _context.UserFriends
                .Where(f => f.UserId == currentUser.Id || f.FriendId == currentUser.Id)
                .Select(f => f.UserId == currentUser.Id ? f.FriendId : f.UserId)
                .ToListAsync();

            Friends = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();

            IncomingRequests = await _context.FriendRequests
             .Where(fr => fr.ReceiverId == currentUser.Id && !fr.IsAccepted)
             .Include(fr => fr.Sender)
             .ToListAsync();

            SentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == currentUser.Id && !fr.IsAccepted)
                .Include(fr => fr.Receiver)
                .ToListAsync();

        }

        [BindProperty]
        public string NewFriendUsername { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnPostSendRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFriendUsername))
            {
                StatusMessage = "Podaj nazwê u¿ytkownika.";
                return RedirectToPage();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByNameAsync(NewFriendUsername);

            if (targetUser == null)
            {
                StatusMessage = "U¿ytkownik nie istnieje.";
                return RedirectToPage();
            }

            if (targetUser.Id == currentUser.Id)
            {
                StatusMessage = "Nie mo¿esz dodaæ samego siebie.";
                return RedirectToPage();
            }

            // Czy zaproszenie ju¿ istnieje?
            bool alreadyExists = await _context.FriendRequests.AnyAsync(fr =>
                (fr.SenderId == currentUser.Id && fr.ReceiverId == targetUser.Id) ||
                (fr.SenderId == targetUser.Id && fr.ReceiverId == currentUser.Id));

            if (alreadyExists)
            {
                StatusMessage = "Zaproszenie ju¿ istnieje lub jesteœcie znajomymi.";
                return RedirectToPage();
            }

            _context.FriendRequests.Add(new FriendRequest
            {
                SenderId = currentUser.Id,
                ReceiverId = targetUser.Id,
                SentAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie wys³ane.";
            return RedirectToPage();
        }

        public List<FriendRequest> SentRequests { get; set; } = new();
        public List<FriendRequest> IncomingRequests { get; set; } = new();

        public async Task<IActionResult> OnPostAcceptRequestAsync(int requestId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == currentUser.Id && !fr.IsAccepted);

            if (request == null)
            {
                StatusMessage = "Zaproszenie nie istnieje lub zosta³o ju¿ zaakceptowane.";
                return RedirectToPage();
            }

            request.IsAccepted = true;

            _context.UserFriends.Add(new UserFriend
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId
            });

            await _context.SaveChangesAsync();

            StatusMessage = "Dodano znajomego!";
            return RedirectToPage();
        }

        




    }



}
