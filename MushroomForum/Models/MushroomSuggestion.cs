namespace MushroomForum.Models
{
    public class MushroomSuggestion
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Probability { get; set; }
        public string Description { get; set; }
        public List<string> CommonNames { get; set; } = new List<string>();
        public string Url { get; set; }
        public string Edibility { get; set; }
        public bool? Psychoactive { get; set; }
        public List<SimilarImage> SimilarImages { get; set; } = new List<SimilarImage>();
    }
}