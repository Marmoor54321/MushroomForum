using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MushroomForum.Data;
using MushroomForum.Models;
using Microsoft.EntityFrameworkCore;


public class UserProfileController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserProfileController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = currentUser.Id,
                Level = 1,
                Experience = 0,
                AvatarIcon = "default.png"
            };
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return View(profile);
    }
}
