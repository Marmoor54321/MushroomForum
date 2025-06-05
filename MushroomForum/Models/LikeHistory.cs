using Microsoft.AspNetCore.Identity;
using MushroomForum.Models;

public class LikeHistory
{
    public int LikeHistoryId { get; set; }

    public string LikerId { get; set; }
    public IdentityUser Liker { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}
