using System;
using System.Collections.Generic;
using System.Linq;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class AnalyticsController
    {
        private readonly ScoreRepository _scoreRepository;
        private readonly TeachRepository _teachRepository;

        private const decimal PassThreshold = 5.0m;

        public AnalyticsController()
        {
            _scoreRepository = new ScoreRepository();
            _teachRepository = new TeachRepository();
        }

        public StatisticsDTO GetStatistics(int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return null;
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return null;
                }

                return _scoreRepository.GetStatistics(classId, subjectId, schoolYear, semester);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, int> GetScoreDistribution(
            int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return new Dictionary<string, int>();
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return new Dictionary<string, int>();
                }

                var scores = _scoreRepository.GetScoreSummaryByClassAndSubject(
                    classId, subjectId, schoolYear, semester);

                if (scores == null || !scores.Any())
                {
                    return new Dictionary<string, int>();
                }

                var distribution = new Dictionary<string, int>
                {
                    { "Xuất Sắc (9.0-10.0)", 0 },
                    { "Giỏi (8.0-8.9)", 0 },
                    { "Khá (7.0-7.9)", 0 },
                    { "Trung Bình (6.0-6.9)", 0 },
                    { "Dưới Trung Bình (5.0-5.9)", 0 },
                    { "Kém (<5.0)", 0 },
                    { "Chưa Nhập", 0 }
                };

                foreach (var score in scores)
                {
                    if (score.FnScore == 0)
                    {
                        distribution["Chưa Nhập"]++;
                    }
                    else if (score.FnScore >= 9.0m)
                    {
                        distribution["Xuất Sắc (9.0-10.0)"]++;
                    }
                    else if (score.FnScore >= 8.0m)
                    {
                        distribution["Giỏi (8.0-8.9)"]++;
                    }
                    else if (score.FnScore >= 7.0m)
                    {
                        distribution["Khá (7.0-7.9)"]++;
                    }
                    else if (score.FnScore >= 6.0m)
                    {
                        distribution["Trung Bình (6.0-6.9)"]++;
                    }
                    else if (score.FnScore >= 5.0m)
                    {
                        distribution["Dưới Trung Bình (5.0-5.9)"]++;
                    }
                    else
                    {
                        distribution["Kém (<5.0)"]++;
                    }
                }

                return distribution;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, int> GetPassFailStatistics(
            int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return new Dictionary<string, int>();
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return new Dictionary<string, int>();
                }

                var scores = _scoreRepository.GetScoreSummaryByClassAndSubject(
                    classId, subjectId, schoolYear, semester);

                if (scores == null || !scores.Any())
                {
                    return new Dictionary<string, int>();
                }

                int passingCount = scores.Count(s => s.FnScore > 0 && s.FnScore >= PassThreshold);
                int failingCount = scores.Count(s => s.FnScore > 0 && s.FnScore < PassThreshold);

                return new Dictionary<string, int>
                {
                    { "Đạt (≥5.0)", passingCount },
                    { "Kém (<5.0)", failingCount }
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, int>();
            }
        }

        public List<ScoreSummaryDTO> GetTopStudents(
            int classId, int subjectId, string schoolYear, int semester, int topN = 10)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return new List<ScoreSummaryDTO>();
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return new List<ScoreSummaryDTO>();
                }

                var scores = _scoreRepository.GetScoreSummaryByClassAndSubject(
                    classId, subjectId, schoolYear, semester);

                if (scores == null || !scores.Any())
                {
                    return new List<ScoreSummaryDTO>();
                }

                return scores
                    .Where(s => s.FnScore > 0)
                    .OrderByDescending(s => s.FnScore)
                    .Take(topN)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<ScoreSummaryDTO>();
            }
        }

        public List<ScoreSummaryDTO> GetAtRiskStudents(
            int classId, int subjectId, string schoolYear, int semester, decimal threshold = PassThreshold)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return new List<ScoreSummaryDTO>();
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return new List<ScoreSummaryDTO>();
                }

                var scores = _scoreRepository.GetScoreSummaryByClassAndSubject(
                    classId, subjectId, schoolYear, semester);

                if (scores == null || !scores.Any())
                {
                    return new List<ScoreSummaryDTO>();
                }

                return scores
                    .Where(s => s.FnScore > 0 && s.FnScore < threshold)
                    .OrderBy(s => s.FnScore)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<ScoreSummaryDTO>();
            }
        }

        public Dictionary<string, decimal> CompareSubjectAverages(int classId, string schoolYear, int semester)
        {
            try
            {
                var classValidation = ValidationHelper.ValidateId(classId, "Class");
                if (!classValidation.isValid)
                {
                    return new Dictionary<string, decimal>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new Dictionary<string, decimal>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new Dictionary<string, decimal>();
                }

                var comparisons = _scoreRepository.GetSubjectAveragesByClass(classId, schoolYear, semester);

                return comparisons?.ToDictionary(c => c.SubjectName, c => c.AverageScore ?? 0)
                    ?? new Dictionary<string, decimal>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, decimal>();
            }
        }

        public Dictionary<string, decimal> GetPerformanceTrend(
            int? studentId, int subjectId, string schoolYear)
        {
            try
            {
                var trend = new Dictionary<string, decimal>();

                for (int semester = 1; semester <= 2; semester++)
                {
                    if (studentId.HasValue)
                    {
                        var scoreDetail = _scoreRepository.GetScoreDetail(
                            studentId.Value, subjectId, schoolYear, semester);

                        if (scoreDetail?.FnScore.HasValue == true)
                        {
                            trend[$"Semester {semester}"] = scoreDetail.FnScore.Value;
                        }
                    }
                }

                return trend;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, decimal>();
            }
        }

        public AnalyticsReportDTO GenerateReport(
            int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!ValidateAnalyticsParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return null;
                }

                if (!CanCurrentUserAnalyze(classId, subjectId, schoolYear, semester))
                {
                    return null;
                }

                var statistics = GetStatistics(classId, subjectId, schoolYear, semester);
                var distribution = GetScoreDistribution(classId, subjectId, schoolYear, semester);
                var passFailStats = GetPassFailStatistics(classId, subjectId, schoolYear, semester);
                var topStudents = GetTopStudents(classId, subjectId, schoolYear, semester, 10);
                var atRiskStudents = GetAtRiskStudents(classId, subjectId, schoolYear, semester);

                if (statistics == null)
                {
                    return null;
                }

                return new AnalyticsReportDTO
                {
                    ClassId = classId,
                    SubjectId = subjectId,
                    SchoolYear = schoolYear,
                    Semester = semester,
                    GeneratedDate = DateTime.Now,
                    Statistics = statistics,
                    ScoreDistribution = distribution,
                    PassFailStatistics = passFailStats,
                    TopPerformers = topStudents,
                    AtRiskStudents = atRiskStudents
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool ValidateAnalyticsParameters(
            int classId, int subjectId, string schoolYear, int semester, out string errorMessage)
        {
            var classValidation = ValidationHelper.ValidateId(classId, "Class");
            if (!classValidation.isValid)
            {
                errorMessage = classValidation.errorMessage;
                return false;
            }

            var subjectValidation = ValidationHelper.ValidateId(subjectId, "Subject");
            if (!subjectValidation.isValid)
            {
                errorMessage = subjectValidation.errorMessage;
                return false;
            }

            var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
            if (!yearValidation.isValid)
            {
                errorMessage = yearValidation.errorMessage;
                return false;
            }

            var semesterValidation = ValidationHelper.ValidateSemester(semester);
            if (!semesterValidation.isValid)
            {
                errorMessage = semesterValidation.errorMessage;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool CanCurrentUserAnalyze(
            int classId, int subjectId, string schoolYear, int semester)
        {
            if (SessionManager.IsAdmin())
            {
                return true;
            }

            if (SessionManager.IsTeacher())
            {
                int? teacherId = SessionManager.GetTeacherId();
                if (!teacherId.HasValue)
                {
                    return false;
                }

                return _teachRepository.IsTeacherAssigned(
                    teacherId.Value, classId, subjectId, schoolYear, semester);
            }

            return false;
        }
    }
}