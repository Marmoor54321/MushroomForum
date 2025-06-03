using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models; // jeśli UserBlock jest tam

namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    public class BlockedUsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BlockedUsersModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<IdentityUser> BlockedUsersList { get; set; } = new();

        [BindProperty]
        public string NewBlockedUsername { get; set; }

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                BlockedUsersList = new List<IdentityUser>();
                return;
            }

            BlockedUsersList = await _context.UserBlocks
                .Where(b => b.BlockerId == currentUser.Id)
                .Include(b => b.Blocked)
                .Select(b => b.Blocked)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUnblockAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage();
            }

            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == currentUser.Id && b.BlockedId == userId);

            if (block != null)
            {
                _context.UserBlocks.Remove(block);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBlockAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(NewBlockedUsername))
            {
                ModelState.AddModelError(string.Empty, "Nazwa użytkownika nie może być pusta.");
                await OnGetAsync();
                return Page();
            }

            var userToBlock = await _userManager.FindByNameAsync(NewBlockedUsername);
            if (userToBlock == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono użytkownika o takiej nazwie.");
                await OnGetAsync();
                return Page();
            }

            if (userToBlock.Id == currentUser.Id)
            {
                ModelState.AddModelError(string.Empty, "Nie możesz zablokować samego siebie.");
                await OnGetAsync();
                return Page();
            }

            // Sprawdź czy blokada już istnieje
            bool alreadyBlocked = await _context.UserBlocks
                .AnyAsync(b => b.BlockerId == currentUser.Id && b.BlockedId == userToBlock.Id);

            if (alreadyBlocked)
            {
                ModelState.AddModelError(string.Empty, "Ten użytkownik jest już zablokowany.");
                await OnGetAsync();
                return Page();
            }

            // Dodaj blokadę
            var block = new UserBlock
            {
                BlockerId = currentUser.Id,
                BlockedId = userToBlock.Id,
                BlockedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(block);

            // Usuń znajomość jeśli istnieje
            var friendRelation = await _context.UserFriends
                .FirstOrDefaultAsync(f =>
                    (f.UserId == currentUser.Id && f.FriendId == userToBlock.Id) ||
                    (f.UserId == userToBlock.Id && f.FriendId == currentUser.Id));

            if (friendRelation != null)
            {
                _context.UserFriends.Remove(friendRelation);
            }

            // Usuń zaproszenia (przychodzące i wysłane)
            var friendRequests = await _context.FriendRequests
                .Where(r => (r.SenderId == currentUser.Id && r.ReceiverId == userToBlock.Id) ||
                            (r.SenderId == userToBlock.Id && r.ReceiverId == currentUser.Id))
                .ToListAsync();

            if (friendRequests.Any())
            {
                _context.FriendRequests.RemoveRange(friendRequests);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
