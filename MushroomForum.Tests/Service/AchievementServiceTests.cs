using Xunit;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Services;
using MushroomForum.Models;
using System.Threading.Tasks;

public class AchievementServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AchievementService _service;

    public AchievementServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // unikalna baza na test
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new AchievementService(_context);
    }

    [Fact]
    public async Task GrantAchievement_AddsAchievementAndGivesExperience_WhenNotExists()
    {
        // Arrange
        var userId = "test-user-id";

        var achievementType = new AchievementType
        {
            Name = "First Post",
            Description = "Test achievement description",
            ExperienceReward = 15
        };
        _context.AchievementTypes.Add(achievementType);

        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 5
        };
        _context.UserProfiles.Add(profile);

        await _context.SaveChangesAsync();

        // Act
        await _service.GrantAchievementIfNotExistsAsync(userId, "First Post");

        // Assert
        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementTypeId == achievementType.Id);

        Assert.NotNull(userAchievement);

        var updatedProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        Assert.NotNull(updatedProfile);

        // Poziomowanie wg formuły:
        // Start: Level=1, Exp=5
        // +15 exp = 20 total
        // Level 1 → 2 wymaga 10 exp → level up, zostaje 10 exp
        // Level 2 → 3 wymaga 25 exp → nie wystarczy (10 < 25)
        Assert.Equal(2, updatedProfile.Level);
        Assert.Equal(10, updatedProfile.Experience);
    }

    [Fact]
    public async Task GrantAchievement_DoesNothing_WhenAchievementAlreadyExists()
    {
        var userId = "test-user-id";

        var achievementType = new AchievementType
        {
            Name = "First Post",
            Description = "Test",
            ExperienceReward = 15
        };
        _context.AchievementTypes.Add(achievementType);

        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 0
        };
        _context.UserProfiles.Add(profile);

        await _context.SaveChangesAsync();

        _context.UserAchievements.Add(new UserAchievement
        {
            UserId = userId,
            AchievementTypeId = achievementType.Id
        });

        await _context.SaveChangesAsync();

        await _service.GrantAchievementIfNotExistsAsync(userId, "First Post");

        var achievementsCount = await _context.UserAchievements.CountAsync(ua => ua.UserId == userId && ua.AchievementTypeId == achievementType.Id);
        Assert.Equal(1, achievementsCount);

        var updatedProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        Assert.Equal(0, updatedProfile.Experience);
    }

    [Fact]
    public async Task GrantAchievement_DoesNothing_WhenAchievementTypeDoesNotExist()
    {
        var userId = "test-user-id";

        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 0
        };
        _context.UserProfiles.Add(profile);

        await _context.SaveChangesAsync();

        await _service.GrantAchievementIfNotExistsAsync(userId, "Nonexistent Achievement");

        var achievementsCount = await _context.UserAchievements.CountAsync(ua => ua.UserId == userId);
        Assert.Equal(0, achievementsCount);

        var updatedProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        Assert.Equal(0, updatedProfile.Experience);
    }

    [Fact]
    public async Task GrantAchievement_IncreasesLevel_WhenExperienceExceedsThreshold()
    {
        var userId = "test-user-id";

        var achievementType = new AchievementType
        {
            Name = "Big Achievement",
            Description = "testbigachievement",
            ExperienceReward = 50
        };
        _context.AchievementTypes.Add(achievementType);

        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 10
        };
        _context.UserProfiles.Add(profile);

        await _context.SaveChangesAsync();

        await _service.GrantAchievementIfNotExistsAsync(userId, "Big Achievement");

        var updatedProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        // Poziomowanie:
        // Start: level 1, exp 10
        // +50 = 60
        // Level 1->2: 10 exp -> level up, exp = 50
        // Level 2->3: 25 exp -> level up, exp = 25
        // Level 3->4: 40 exp -> not enough (25 < 40)
        Assert.Equal(3, updatedProfile.Level);
        Assert.Equal(25, updatedProfile.Experience);
    }
}
