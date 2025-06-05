using MushroomForum.Data;
using MushroomForum.Models;
using Microsoft.EntityFrameworkCore;

namespace MushroomForum.Services
{
    public class FriendService
    {
        private readonly ApplicationDbContext _context;

        public FriendService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendFriendRequestAsync(string senderId, string receiverId)
        {
            if (senderId == receiverId)
                throw new Exception("Nie możesz wysłać prośby do siebie.");

            // Sprawdź, czy już są znajomymi
            bool areFriends = await _context.UserFriends.AnyAsync(f =>
                (f.UserId == senderId && f.FriendId == receiverId) ||
                (f.UserId == receiverId && f.FriendId == senderId));

            if (areFriends)
                throw new Exception("Użytkownicy są już znajomymi.");

            // Sprawdź, czy prośba już istnieje
            bool requestExists = await _context.FriendRequests.AnyAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && !fr.IsAccepted);

            if (requestExists)
                throw new Exception("Prośba została już wysłana.");

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId
            };



            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();
        }
    }
}
