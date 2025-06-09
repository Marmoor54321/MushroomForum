using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.Services.Service;
using Xunit;

public class LevelUpServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly LevelUpService _service;

    public LevelUpServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new LevelUpService(_context);
    }

    [Fact]
    public async Task AddExperienceAsync_IncreasesExperienceWithoutLevelUp_WhenNotEnoughExp()
    {
        // Arrange
        var userId = "user1";
        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 0
        };
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddExperienceAsync(userId, 5);

        // Assert
        var updated = await _context.UserProfiles.FirstAsync(p => p.UserId == userId);
        Assert.Equal(1, updated.Level);
        Assert.Equal(5, updated.Experience);
    }

    [Fact]
    public async Task AddExperienceAsync_IncreasesLevel_WhenExpExceedsThreshold()
    {
        // Arrange
        var userId = "user2";
        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 5
        };
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddExperienceAsync(userId, 20); // total 25 exp

        // Calculate expected levels and leftover exp:
        // Level 1 to 2 needs: 10 * 1 + 5*(1-1) = 10 exp
        // Level 2 to 3 needs: 10 * 2 + 5*(2-1) = 25 exp
        // Total exp after add: 5 + 20 = 25

        // Process:
        // 25 >= 10 -> level up to 2, exp left = 15
        // 15 < 25 -> stop leveling, level = 2, exp = 15

        // Assert
        var updated = await _context.UserProfiles.FirstAsync(p => p.UserId == userId);
        Assert.Equal(2, updated.Level);
        Assert.Equal(15, updated.Experience);
    }

    [Fact]
    public async Task AddExperienceAsync_LevelsUpMultipleTimes_WhenExpIsHigh()
    {
        // Arrange
        var userId = "user3";
        var profile = new UserProfile
        {
            UserId = userId,
            Level = 1,
            Experience = 0
        };
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddExperienceAsync(userId, 100);

        // Level thresholds:
        // From 1 to 2: 10
        // From 2 to 3: 25
        // From 3 to 4: 40
        // From 4 to 5: 55
        // Sum to reach level 5: 10 + 25 + 40 + 55 = 130 exp needed
        // We have only 100 exp, so:
        // 100 >= 10 -> level 2, left 90
        // 90 >= 25 -> level 3, left 65
        // 65 >= 40 -> level 4, left 25
        // 25 < 55 stop
        // Expected level = 4, experience left = 25

        var updated = await _context.UserProfiles.FirstAsync(p => p.UserId == userId);
        Assert.Equal(4, updated.Level);
        Assert.Equal(25, updated.Experience);
    }

    [Fact]
    public async Task AddExperienceAsync_DoesNothing_WhenProfileNotFound()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        await _service.AddExperienceAsync(userId, 50);

        // Assert: No exception, no changes in db
        var profilesCount = await _context.UserProfiles.CountAsync();
        Assert.Equal(0, profilesCount);
    }
}
