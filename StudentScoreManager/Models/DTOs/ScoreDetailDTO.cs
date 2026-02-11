namespace StudentScoreManager.Models.DTOs
{
    public class ScoreDetailDTO
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

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
                return System.Math.Round((QtScore.Value * 0.2m) + (GkScore.Value * 0.4m) + (CkScore.Value * 0.4m), 2);
            }
            return null;
        }
    }
}