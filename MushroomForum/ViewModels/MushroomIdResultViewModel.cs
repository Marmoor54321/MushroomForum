using System.Collections.Generic;

namespace MushroomForum.Models
{
    public class MushroomIdResultViewModel
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<MushroomSuggestion> Suggestions { get; set; } = new List<MushroomSuggestion>();
        public bool IsMushroom { get; set; }
        public double MushroomProbability { get; set; }
    }
}