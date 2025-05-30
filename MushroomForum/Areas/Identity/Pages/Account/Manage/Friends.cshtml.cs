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
using MushroomForum.Services;

namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class FriendsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FriendService _friendService;

        public FriendsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context, FriendService friendService)
        {
            _userManager = userManager;
            _context = context;
            _friendService = friendService;
        }

        public async Task<IActionResult> OnPostRemoveFriendAsync(string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                StatusMessage = "Nie wybrano znajomego do usuni�cia.";
                return RedirectToPage();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Znajd� rekord znajomo�ci (UserFriend)
            var friendship = await _context.UserFriends
                .FirstOrDefaultAsync(f =>
                    (f.UserId == currentUser.Id && f.FriendId == friendId) ||
                    (f.FriendId == currentUser.Id && f.UserId == friendId));

            if (friendship == null)
            {
                StatusMessage = "Nie znaleziono znajomego do usuni�cia.";
                return RedirectToPage();
            }

            _context.UserFriends.Remove(friendship);

            // Znajd� powi�zany rekord FriendRequest, kt�ry jest zaakceptowany i ��czy tych samych u�ytkownik�w
            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    ((fr.SenderId == currentUser.Id && fr.ReceiverId == friendId) ||
                     (fr.SenderId == friendId && fr.ReceiverId == currentUser.Id))
                    && fr.IsAccepted);

            if (friendRequest != null)
            {
                _context.FriendRequests.Remove(friendRequest);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Usuni�to u�ytkownika ze znajomych.";
            return RedirectToPage();
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
                StatusMessage = "Podaj nazw� u�ytkownika.";
                return RedirectToPage();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByNameAsync(NewFriendUsername);

            if (targetUser == null)
            {
                StatusMessage = "U�ytkownik nie istnieje.";
                return RedirectToPage();
            }

            if (targetUser.Id == currentUser.Id)
            {
                StatusMessage = "Nie mo�esz doda� samego siebie.";
                return RedirectToPage();
            }

            // --- Sprawdzenie blokad ---
            bool isBlocked = await _context.UserBlocks.AnyAsync(b =>
                (b.BlockerId == currentUser.Id && b.BlockedId == targetUser.Id) ||
                (b.BlockerId == targetUser.Id && b.BlockedId == currentUser.Id));

            if (isBlocked)
            {
                StatusMessage = "Nie mo�esz wys�a� zaproszenia, poniewa� ty lub ten u�ytkownik jest zablokowany.";
                return RedirectToPage();
            }

            // Czy zaproszenie ju� istnieje?
            bool alreadyExists = await _context.FriendRequests.AnyAsync(fr =>
                (fr.SenderId == currentUser.Id && fr.ReceiverId == targetUser.Id) ||
                (fr.SenderId == targetUser.Id && fr.ReceiverId == currentUser.Id));

            if (alreadyExists)
            {
                StatusMessage = "Zaproszenie ju� istnieje lub jeste�cie znajomymi.";
                return RedirectToPage();
            }

            _context.FriendRequests.Add(new FriendRequest
            {
                SenderId = currentUser.Id,
                ReceiverId = targetUser.Id,
                SentAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie wys�ane.";
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
                StatusMessage = "Zaproszenie nie istnieje lub zosta�o ju� zaakceptowane.";
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

        public async Task<IActionResult> OnPostCancelRequestAsync(int requestId)
        {
            var request = await _context.FriendRequests.FindAsync(requestId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (request == null || request.SenderId != currentUser.Id || request.IsAccepted)
            {
                StatusMessage = "Nieprawid�owe zaproszenie do cofni�cia.";
                return RedirectToPage();
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie zosta�o cofni�te.";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostRejectRequestAsync(int requestId)
        {
            var request = await _context.FriendRequests.FindAsync(requestId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (request == null || request.ReceiverId != currentUser.Id || request.IsAccepted)
            {
                StatusMessage = "Nieprawid�owe zaproszenie do odrzucenia.";
                return RedirectToPage();
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie zosta�o odrzucone.";
            return RedirectToPage();
        }


        





    }



}
