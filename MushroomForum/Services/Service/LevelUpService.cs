using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;



namespace MushroomForum.Services.Service
{
    
    public class LevelUpService
    {
        private readonly ApplicationDbContext _context;

        public LevelUpService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddExperienceAsync(string userId, int amount)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return;

            profile.Experience += amount;

            while (profile.Experience >= profile.ExperienceToNextLevel)
            {
                profile.Experience -= profile.ExperienceToNextLevel;
                profile.Level++;
            }

            await _context.SaveChangesAsync();
        }
        public async Task GiveExperienceAsync(string userId, int amount)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return;

            profile.Experience += amount;

            // Sprawdzamy, czy należy zwiększyć poziom
            while (profile.Experience >= GetExperienceForNextLevel(profile.Level))
            {
                profile.Experience -= GetExperienceForNextLevel(profile.Level);
                profile.Level++;
            }

            await _context.SaveChangesAsync();
        }

        private int GetExperienceForNextLevel(int level)
        {
            return 10 * level + (5 * (level - 1));
        }

    }
}
