namespace StudentScoreManager.Models.DTOs
{
    public class ScoreSummaryDTO
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public decimal? FinalScore { get; set; }

        public decimal? FnScore { get; set; }

        public string FnScoreDisplay => FnScore.HasValue ? FnScore.Value.ToString("F2") : "Not Graded";

        public bool IsAtRisk => FnScore.HasValue && FnScore.Value < 5.0m;

        public string ScoreColor
        {
            get
            {
                if (!FnScore.HasValue) return "Gray";
                return FnScore.Value >= 5.0m ? "Green" : "Red";
            }
        }

        public bool IsTopPerformer => FnScore.HasValue && FnScore.Value >= 9.0m;

        public string FinalScoreDisplay => FinalScore.HasValue ? FinalScore.Value.ToString("F2") : "Not Graded";

        public string PerformanceLevel
        {
            get
            {
                if (!FinalScore.HasValue) return "N/A";
                return FinalScore.Value switch
                {
                    >= 9.0m => "Excellent",
                    >= 8.0m => "Very Good",
                    >= 6.5m => "Good",
                    >= 5.0m => "Average",
                    _ => "Weak"
                };
            }
        }

        public string FnPerformanceLevel
        {
            get
            {
                if (!FnScore.HasValue) return "N/A";
                return FnScore.Value switch
                {
                    >= 9.0m => "Excellent",
                    >= 8.0m => "Very Good",
                    >= 6.5m => "Good",
                    >= 5.0m => "Average",
                    _ => "Weak"
                };
            }
        }
    }
}
