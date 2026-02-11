using System;

namespace StudentScoreManager.Models.Entities
{
    public class Score
    {
        public int StudentId { get; set; }

        public int SubjectId { get; set; }

        public string SchoolYear { get; set; }

        public int Semester { get; set; }

        public decimal? QtScore { get; set; }

        public decimal? GkScore { get; set; }

        public decimal? CkScore { get; set; }

        public decimal? FnScore { get; set; }

        public decimal? CalculateFinalScore()
        {
            if (QtScore.HasValue && GkScore.HasValue && CkScore.HasValue)
            {
                return Math.Round((QtScore.Value * 0.2m) + (GkScore.Value * 0.4m) + (CkScore.Value * 0.4m), 2);
            }
            return null;
        }

        public bool IsPassing => FnScore.HasValue && FnScore.Value >= 5.0m;

        public string PerformanceLevel
        {
            get
            {
                if (!FnScore.HasValue) return "Not Graded";
                return FnScore.Value switch
                {
                    >= 9.0m => "Excellent (Xuất sắc)",
                    >= 8.0m => "Very Good (Giỏi)",
                    >= 6.5m => "Good (Khá)",
                    >= 5.0m => "Average (Trung bình)",
                    _ => "Weak (Yếu)"
                };
            }
        }
    }
}