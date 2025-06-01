namespace MushroomForum.ViewModels
{
    public class CombinedRankingViewModel
    {
        public YourRankingViewModel YourRanking { get; set; }
        public List<RankingViewModel> FullRanking { get; set; }
    }
}
