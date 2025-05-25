using System;
using System.ComponentModel.DataAnnotations;

namespace MushroomForum.Models
{
    public class MushroomSpot
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
