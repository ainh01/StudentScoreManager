using System;
using System.Collections.Generic;

namespace StudentScoreManager.Models.DTOs
{
    public class AnalyticsReportDTO
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public string SchoolYear { get; set; }
        public int Semester { get; set; }
        public DateTime GeneratedDate { get; set; }

        public StatisticsDTO Statistics { get; set; }

        public Dictionary<string, int> ScoreDistribution { get; set; }
        public Dictionary<string, int> PassFailStatistics { get; set; }

        public List<ScoreSummaryDTO> TopPerformers { get; set; }
        public List<ScoreSummaryDTO> AtRiskStudents { get; set; }

        public decimal PassRate => Statistics?.GradedStudents > 0
            ? (Statistics.PassCount / (decimal)Statistics.GradedStudents) * 100
            : 0;

        public decimal FailRate => Statistics?.GradedStudents > 0
            ? (Statistics.FailCount / (decimal)Statistics.GradedStudents) * 100
            : 0;

        public string PerformanceSummary
        {
            get
            {
                if (Statistics == null || Statistics.GradedStudents == 0)
                    return "Thiếu dữ liệu.";

                if (!Statistics.AverageScore.HasValue)
                    return "Insufficient grading data.";

                if (Statistics.AverageScore.Value >= 8.0m)
                    return "Lớp học lực Xuất sắc";
                else if (Statistics.AverageScore.Value >= 6.5m)
                    return "Lớp học lực Giỏi.";
                else if (Statistics.AverageScore.Value >= 5.0m)
                    return "Lớp học lực Ổn";
                else
                    return "Học lực của lớp cần được cải thiện.";
            }
        }
    }
}