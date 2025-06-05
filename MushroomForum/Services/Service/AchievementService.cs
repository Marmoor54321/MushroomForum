using MushroomForum.Data;
using MushroomForum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MushroomForum.Services
{
    public class AchievementService
    {
        private readonly ApplicationDbContext _context;

        public AchievementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GrantAchievementIfNotExistsAsync(string userId, string achievementName)
        {
            var type = await _context.AchievementTypes.FirstOrDefaultAsync(a => a.Name == achievementName);
            if (type == null) return;

            var alreadyHas = await _context.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementTypeId == type.Id);

            if (alreadyHas) return;

            _context.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementTypeId = type.Id
            });

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                profile.Experience += type.ExperienceReward;

                while (profile.Experience >= GetExperienceForNextLevel(profile.Level))
                {
                    profile.Experience -= GetExperienceForNextLevel(profile.Level);
                    profile.Level++;
                }
            }

            await _context.SaveChangesAsync();
        }

        private int GetExperienceForNextLevel(int level)
        {
            return 10 * level + (5 * (level - 1));
        }
    }
}
