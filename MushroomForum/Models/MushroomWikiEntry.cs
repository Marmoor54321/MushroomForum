using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MushroomForum.Models
{
    public class MushroomWikiEntry
    {
        public int Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string LatinName { get; set; }
        [MaxLength(64)]
        public string Description { get; set; }
        [MaxLength(32)]
        public string Type { get; set; } // 0 - edible, 1 - inedible, 2 - poisonous, 3 - hallucinogenic
        public DateTime Date { get; set; } = DateTime.Now;
        public string? PhotoUrl { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
    }
}