using MushroomForum.Models;

namespace MushroomForum.ViewModels
{
    public class MushroomHarvestIndexViewModel
    {
        public List<MushroomHarvestEntry> Entries { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalEntries { get; set; }
    }
}
