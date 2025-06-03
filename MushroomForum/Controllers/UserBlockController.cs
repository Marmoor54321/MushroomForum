using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize]
public class UserBlockController : Controller
{
    private readonly UserBlockService _blockService;
    private readonly UserManager<IdentityUser> _userManager;

    public UserBlockController(UserBlockService blockService, UserManager<IdentityUser> userManager)
    {
        _blockService = blockService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);
        var blockedUsers = await _blockService.GetBlockedUsersAsync(currentUserId);
        return View(blockedUsers); // Widok z listą zablokowanych
    }

    [HttpPost]
    public async Task<IActionResult> Block(string blockedUserId)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (await _blockService.BlockUserAsync(currentUserId, blockedUserId))
        {
            TempData["Message"] = "Użytkownik zablokowany";
        }
        else
        {
            TempData["Error"] = "Nie można zablokować tego użytkownika";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(string blockedUserId)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (await _blockService.UnblockUserAsync(currentUserId, blockedUserId))
        {
            TempData["Message"] = "Użytkownik odblokowany";
        }
        else
        {
            TempData["Error"] = "Nie można odblokować tego użytkownika";
        }
        return RedirectToAction(nameof(Index));
    }
}
