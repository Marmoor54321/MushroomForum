namespace MushroomForum.Models
{
    public class SimilarImage
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string UrlSmall { get; set; }
        public string LicenseName { get; set; }
        public string LicenseUrl { get; set; }
        public string Citation { get; set; }
        public double Similarity { get; set; }
    }
}