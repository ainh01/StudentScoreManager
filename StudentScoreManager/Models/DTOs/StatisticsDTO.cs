using System;

namespace StudentScoreManager.Models.DTOs
{
    public class StatisticsDTO
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int TotalStudents { get; set; }

        public int GradedStudents { get; set; }

        public decimal? AverageScore { get; set; }

        public decimal? MedianScore { get; set; }

        public decimal? StandardDeviation { get; set; }

        public decimal? MinScore { get; set; }

        public decimal? MaxScore { get; set; }

        public int PassCount { get; set; }

        public int FailCount { get; set; }

        public decimal PassRate => GradedStudents > 0
            ? Math.Round((decimal)PassCount / GradedStudents * 100, 2)
            : 0;

        public decimal FailRate => GradedStudents > 0
            ? Math.Round((decimal)FailCount / GradedStudents * 100, 2)
            : 0;

        public decimal HighestScore => MaxScore ?? 0.0m;

        public decimal LowestScore => MinScore ?? 0.0m;

        public int PassingStudents => PassCount;

        public int FailingStudents => FailCount;
    }
}
