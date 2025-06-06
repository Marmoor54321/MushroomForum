using System.ComponentModel.DataAnnotations;

public class AchievementType
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } 

    public string Description { get; set; }

    public int ExperienceReward { get; set; } = 0;

    public string? UnlocksAvatarIcon { get; set; } 
}
