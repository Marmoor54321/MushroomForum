using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MushroomForum.Services
{
    public class UserBlockService
    {
        private readonly ApplicationDbContext _context;

        public UserBlockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<List<UserBlock>> GetBlockedUsersAsync(string userId)
        {
            return await _context.UserBlocks
                .Where(b => b.BlockerId == userId)
                .Include(b => b.Blocked)
                .ToListAsync();
        }

        public virtual async Task<bool> BlockUserAsync(string blockerId, string blockedId)
        {
            if (_context.UserBlocks.Any(b => b.BlockerId == blockerId && b.BlockedId == blockedId))
                return false;

            _context.UserBlocks.Add(new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = blockedId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> UnblockUserAsync(string blockerId, string blockedId)
        {
            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

            if (block == null)
                return false;

            _context.UserBlocks.Remove(block);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
