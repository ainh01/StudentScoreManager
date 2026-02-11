namespace StudentScoreManager.Models.DTOs
{
    public class StudentScoreDTO
    {
        public string SubjectName { get; set; }

        public decimal? QtScore { get; set; }

        public decimal? GkScore { get; set; }

        public decimal? CkScore { get; set; }

        public decimal? FnScore { get; set; }

        public string QtDisplay => QtScore?.ToString("F2") ?? "-";
        public string GkDisplay => GkScore?.ToString("F2") ?? "-";
        public string CkDisplay => CkScore?.ToString("F2") ?? "-";
        public string FnDisplay => FnScore?.ToString("F2") ?? "-";

        public bool IsPassing => FnScore.HasValue && FnScore.Value >= 5.0m;

        public string Status => FnScore.HasValue ? (IsPassing ? "Pass" : "Fail") : "Incomplete";
    }
}