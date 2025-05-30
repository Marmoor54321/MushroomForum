using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MushroomForum.Data;

public class UserBlockService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public UserBlockService(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<bool> BlockUserAsync(string blockerId, string blockedId)
    {
        if (blockerId == blockedId)
            return false; // Nie można zablokować siebie

        var alreadyBlocked = await _dbContext.UserBlocks
            .AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

        if (alreadyBlocked)
            return false; // Już zablokowany

        var block = new UserBlock
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            BlockedAt = DateTime.UtcNow
        };

        _dbContext.UserBlocks.Add(block);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnblockUserAsync(string blockerId, string blockedId)
    {
        var block = await _dbContext.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

        if (block == null)
            return false;

        _dbContext.UserBlocks.Remove(block);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<IdentityUser>> GetBlockedUsersAsync(string blockerId)
    {
        var blockedUsers = await _dbContext.UserBlocks
            .Where(b => b.BlockerId == blockerId)
            .Include(b => b.Blocked)
            .Select(b => b.Blocked)
            .ToListAsync();

        return blockedUsers;
    }
}
