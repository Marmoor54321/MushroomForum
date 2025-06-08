using MushroomForum.Data;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Services.Service;

public class DailyQuestService
{
    private readonly ApplicationDbContext _context;

    private readonly LevelUpService _levelUpService;

    public DailyQuestService(ApplicationDbContext context, LevelUpService levelUpService)
    {
        _context = context;
        _levelUpService = levelUpService;
    }



    const int RewardExperience = 50;
    public async Task UpdateProgressAsync(string userId, string questType, int increment = 1)
    {
        var today = DateTime.UtcNow.Date;
        var currentDayOfWeek = DateTime.UtcNow.DayOfWeek;

        // Znajdź progres użytkownika dla danego questa na dzisiaj
        var progress = await _context.DailyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.QuestType == questType && p.Date == today);

        if (progress == null)
        {
            return;
        }

        if (progress.Completed)
        {
            return;
        }

        // Znajdź definicję questa (target) dla dzisiejszego dnia tygodnia
        var questTypeEntry = await _context.DailyQuestTypes
            .FirstOrDefaultAsync(q => q.QuestType == questType && q.DayOfWeek == currentDayOfWeek);

        if (questTypeEntry == null)
        {
            return;
        }

        // Zwiększ progress
        progress.Progress += increment;

        // Sprawdź czy quest jest ukończony
        if (progress.Progress >= questTypeEntry.Target && !progress.Completed)
        {
            progress.Completed = true;

            await _levelUpService.GiveExperienceAsync(userId, RewardExperience);

        }

        await _context.SaveChangesAsync();
    }



    public async Task<bool> IsQuestCompletedAsync(string userId, string questType)
    {
        var today = DateTime.UtcNow.Date;

        var progress = await _context.DailyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.QuestType == questType && p.Date == today);

        return progress != null && progress.Completed;
    }


}
