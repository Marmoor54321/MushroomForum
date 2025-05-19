using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class Friend
    {
        //klucz glowny znajomosci
        [Key]
        public int Id { get; set; }

        //id tego kto wyslal zaproszenie
        [Required]
        public string UserId { get; set; }

        //id tego kto otrzymal zaproszenie
        [Required]
        public string FriendUserId { get; set; }

        //status znajomości, np. "Pending", "Accepted", "Blocked"
        [Required]
        public string Status { get; set; } = "Pending";

        // Nawigacja
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [ForeignKey("FriendUserId")]
        public IdentityUser? FriendUser { get; set; }
    }
}

