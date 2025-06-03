public class UserFriend
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public string FriendId { get; set; }

    public DateTime FriendsSince { get; set; } = DateTime.UtcNow;
}
