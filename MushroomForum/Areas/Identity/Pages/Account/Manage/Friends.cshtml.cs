using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.Services;
using System;

namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class FriendsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FriendService _friendService;
        private readonly AchievementService _achievementService;

        public FriendsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context, FriendService friendService, AchievementService achievementService)
        {
            _userManager = userManager;
            _context = context;
            _friendService = friendService;
            _achievementService = achievementService;
        }

        public class FriendViewModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string AvatarIcon { get; set; }
        }

        public class FriendRequestViewModel
        {
            public int RequestId { get; set; }       // Id samego zaproszenia
            public string UserId { get; set; }       // Id u¿ytkownika (SenderId lub ReceiverId)
            public string UserName { get; set; }
            public string AvatarIcon { get; set; }
        }

        public List<FriendViewModel> FriendsWithIcons { get; set; } = new();

        // Nowe listy z avatarami dla zaproszeñ
        public List<FriendRequestViewModel> IncomingRequestsWithIcons { get; set; } = new();
        public List<FriendRequestViewModel> SentRequestsWithIcons { get; set; } = new();


        public List<IdentityUser> Friends { get; set; } = new();

        public List<FriendRequest> SentRequests { get; set; } = new();
        public List<FriendRequest> IncomingRequests { get; set; } = new();

        [BindProperty]
        public string NewFriendUsername { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnPostRemoveFriendAsync(string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                StatusMessage = "Nie wybrano znajomego do usuniêcia.";
                return RedirectToPage();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var friendship = await _context.UserFriends
                .FirstOrDefaultAsync(f =>
                    (f.UserId == currentUser.Id && f.FriendId == friendId) ||
                    (f.FriendId == currentUser.Id && f.UserId == friendId));

            if (friendship == null)
            {
                StatusMessage = "Nie znaleziono znajomego do usuniêcia.";
                return RedirectToPage();
            }

            _context.UserFriends.Remove(friendship);

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

            StatusMessage = "Usuniêto u¿ytkownika ze znajomych.";
            return RedirectToPage();
        }

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.Id;

            var friendIds = await _context.UserFriends
                .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
                .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
                .ToListAsync();

            var friends = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();

            var friendProfiles = await _context.UserProfiles
                .Where(up => friendIds.Contains(up.UserId))
                .ToListAsync();

            FriendsWithIcons = friends.Select(f =>
            {
                var profile = friendProfiles.FirstOrDefault(p => p.UserId == f.Id);
                return new FriendViewModel
                {
                    Id = f.Id,
                    UserName = f.UserName,
                    AvatarIcon = profile?.AvatarIcon ?? "default.png"
                };
            }).ToList();
            if (friendIds.Count > 0)
            {
                await _achievementService.GrantAchievementIfNotExistsAsync(currentUser.Id, "FirstFriend");
            }

            // Przychodz¹ce zaproszenia
            var incomingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == currentUserId && !fr.IsAccepted)
                .Include(fr => fr.Sender)
                .ToListAsync();

            var incomingSenderIds = incomingRequests.Select(fr => fr.SenderId).ToList();
            var incomingSenderProfiles = await _context.UserProfiles
                .Where(up => incomingSenderIds.Contains(up.UserId))
                .ToListAsync();

            IncomingRequestsWithIcons = incomingRequests.Select(fr => new FriendRequestViewModel
            {
                RequestId = fr.Id,
                UserId = fr.SenderId,
                UserName = fr.Sender.UserName,
                AvatarIcon = incomingSenderProfiles.FirstOrDefault(p => p.UserId == fr.SenderId)?.AvatarIcon ?? "default.png"
            }).ToList();

            // Wys³ane zaproszenia
            var sentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == currentUserId && !fr.IsAccepted)
                .Include(fr => fr.Receiver)
                .ToListAsync();

            var sentReceiverIds = sentRequests.Select(fr => fr.ReceiverId).ToList();
            var sentReceiverProfiles = await _context.UserProfiles
                .Where(up => sentReceiverIds.Contains(up.UserId))
                .ToListAsync();

            SentRequestsWithIcons = sentRequests.Select(fr => new FriendRequestViewModel
            {
                RequestId = fr.Id,
                UserId = fr.ReceiverId,
                UserName = fr.Receiver.UserName,
                AvatarIcon = sentReceiverProfiles.FirstOrDefault(p => p.UserId == fr.ReceiverId)?.AvatarIcon ?? "default.png"
            }).ToList();
        }


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

            bool isBlocked = await _context.UserBlocks.AnyAsync(b =>
                (b.BlockerId == currentUser.Id && b.BlockedId == targetUser.Id) ||
                (b.BlockerId == targetUser.Id && b.BlockedId == currentUser.Id));

            if (isBlocked)
            {
                StatusMessage = "Nie mo¿esz wys³aæ zaproszenia, poniewa¿ ty lub ten u¿ytkownik jest zablokowany.";
                return RedirectToPage();
            }

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
            await _achievementService.GrantAchievementIfNotExistsAsync(currentUser.Id, "FirstFriend");
            await _achievementService.GrantAchievementIfNotExistsAsync(request.SenderId, "FirstFriend");
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
                StatusMessage = "Nieprawid³owe zaproszenie do cofniêcia.";
                return RedirectToPage();
            }

            
            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie zosta³o cofniête.";
            return RedirectToPage();


        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int requestId)
        {
            var request = await _context.FriendRequests.FindAsync(requestId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (request == null || request.ReceiverId != currentUser.Id || request.IsAccepted)
            {
                StatusMessage = "Nieprawid³owe zaproszenie do odrzucenia.";
                return RedirectToPage();
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            StatusMessage = "Zaproszenie zosta³o odrzucone.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBlockUserAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
            {
                StatusMessage = "Nie mo¿na zablokowaæ u¿ytkownika.";
                return Page();
            }

            if (userId == currentUser.Id)
            {
                StatusMessage = "Nie mo¿esz zablokowaæ samego siebie.";
                return Page();
            }

            var existingBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == currentUser.Id && b.BlockedId == userId);

            if (existingBlock != null)
            {
                StatusMessage = "U¿ytkownik jest ju¿ zablokowany.";
                return Page();
            }

            var userToBlock = await _userManager.FindByIdAsync(userId);
            if (userToBlock == null)
            {
                StatusMessage = "Nie znaleziono u¿ytkownika do zablokowania.";
                return Page();
            }

            var block = new UserBlock
            {
                BlockerId = currentUser.Id,
                BlockedId = userId,
                BlockedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(block);

            var friendRelation = await _context.UserFriends
                .FirstOrDefaultAsync(f =>
                    (f.UserId == currentUser.Id && f.FriendId == userId) ||
                    (f.UserId == userId && f.FriendId == currentUser.Id));

            if (friendRelation != null)
            {
                _context.UserFriends.Remove(friendRelation);
            }

            var friendRequests = await _context.FriendRequests
                .Where(r => (r.SenderId == currentUser.Id && r.ReceiverId == userId) ||
                            (r.SenderId == userId && r.ReceiverId == currentUser.Id))
                .ToListAsync();

            if (friendRequests.Any())
            {
                _context.FriendRequests.RemoveRange(friendRequests);
            }

            await _context.SaveChangesAsync();

            StatusMessage = $"Zablokowano u¿ytkownika {userToBlock.UserName}.";
            return RedirectToPage();
        }
    }
}
