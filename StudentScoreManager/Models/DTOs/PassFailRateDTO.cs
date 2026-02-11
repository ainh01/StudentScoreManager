namespace StudentScoreManager.Models.DTOs
{
    public class PassFailRateDTO
    {
        public int PassCount { get; set; }

        public int FailCount { get; set; }

        public int Total => PassCount + FailCount;

        public decimal PassPercentage => Total > 0 ? (decimal)PassCount / Total * 100 : 0;

        public decimal FailPercentage => Total > 0 ? (decimal)FailCount / Total * 100 : 0;
    }
}