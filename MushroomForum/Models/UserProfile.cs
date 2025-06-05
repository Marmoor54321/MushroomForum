using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int ExperienceToNextLevel => 10 * Level + (5 * (Level - 1));

        public string AvatarIcon { get; set; } = "default.png"; // np. nazwa pliku ikony
    }
}
