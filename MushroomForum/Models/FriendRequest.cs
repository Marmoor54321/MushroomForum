using Microsoft.AspNetCore.Identity;

public class FriendRequest
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public IdentityUser Sender { get; set; }  // potrzebne!
    public string ReceiverId { get; set; }
    public IdentityUser Receiver { get; set; }  // opcjonalnie
    public bool IsAccepted { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
