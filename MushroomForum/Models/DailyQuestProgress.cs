using Microsoft.AspNetCore.Identity;

public class DailyQuestProgress
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }

    public DateTime Date { get; set; } 

    public string QuestType { get; set; } 
    public int Progress { get; set; }
    public bool Completed { get; set; }
}
