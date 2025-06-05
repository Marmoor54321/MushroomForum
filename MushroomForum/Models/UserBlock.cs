using Microsoft.AspNetCore.Identity;
using System;

public class UserBlock
{
    public int Id { get; set; }

    public string BlockerId { get; set; }
    public IdentityUser Blocker { get; set; }

    public string BlockedId { get; set; }
    public IdentityUser Blocked { get; set; }

    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

}
