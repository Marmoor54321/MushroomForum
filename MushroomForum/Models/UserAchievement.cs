using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class UserAchievement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public IdentityUser User { get; set; }

    [Required]
    public int AchievementTypeId { get; set; }

    [ForeignKey("AchievementTypeId")]
    public AchievementType AchievementType { get; set; }

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}
