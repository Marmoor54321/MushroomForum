using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MushroomForum.Services;

namespace MushroomForum.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly FriendService _friendService;

        public FriendController(FriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            try
            {
                await _friendService.SendFriendRequestAsync(currentUserId, userId);
                return Ok("Prośba wysłana.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
