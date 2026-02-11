using System;
using System.Collections.Generic;
using System.Linq;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class ScoreController
    {
        private readonly ScoreRepository _scoreRepository;
        private readonly StudentRepository _studentRepository;
        private readonly TeachRepository _teachRepository;

        private const decimal QtWeight = 0.2m;
        private const decimal GkWeight = 0.4m;
        private const decimal CkWeight = 0.4m;

        public ScoreController()
        {
            _scoreRepository = new ScoreRepository();
            _studentRepository = new StudentRepository();
            _teachRepository = new TeachRepository();
        }

        public decimal? CalculateFinalScore(decimal? qtScore, decimal? gkScore, decimal? ckScore)
        {
            if (!qtScore.HasValue && !gkScore.HasValue && !ckScore.HasValue)
            {
                return null;
            }

            decimal qt = qtScore ?? 0;
            decimal gk = gkScore ?? 0;
            decimal ck = ckScore ?? 0;

            decimal finalScore = (qt * QtWeight) + (gk * GkWeight) + (ck * CkWeight);

            return Math.Round(finalScore, 2);
        }

        public List<ScoreSummaryDTO> GetScoreSummary(
            int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!ValidateScoreQueryParameters(classId, subjectId, schoolYear, semester, out string error))
                {
                    return new List<ScoreSummaryDTO>();
                }

                if (!CanCurrentUserAccessScores(classId, subjectId, schoolYear, semester))
                {
                    return new List<ScoreSummaryDTO>();
                }

                var summary = _scoreRepository.GetScoreSummaryByClassAndSubject(
                    classId, subjectId, schoolYear, semester);

                return summary?.ToList() ?? new List<ScoreSummaryDTO>();
            }
            catch (Exception ex)
            {
                return new List<ScoreSummaryDTO>();
            }
        }

        public ScoreDetailDTO GetScoreDetail(
            int studentId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                var studentValidation = ValidationHelper.ValidateId(studentId, "Student");
                if (!studentValidation.isValid)
                {
                    return null;
                }

                var subjectValidation = ValidationHelper.ValidateId(subjectId, "Subject");
                if (!subjectValidation.isValid)
                {
                    return null;
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return null;
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return null;
                }

                if (!SessionManager.CanAccessStudent(studentId))
                {
                    return null;
                }

                return _scoreRepository.GetScoreDetail(studentId, subjectId, schoolYear, semester);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public (bool success, string message) SaveScore(
            int studentId, int subjectId, string schoolYear, int semester,
            decimal? qtScore, decimal? gkScore, decimal? ckScore)
        {
            try
            {
                var studentValidation = ValidationHelper.ValidateId(studentId, "Student");
                if (!studentValidation.isValid)
                {
                    return (false, studentValidation.errorMessage);
                }

                var subjectValidation = ValidationHelper.ValidateId(subjectId, "Subject");
                if (!subjectValidation.isValid)
                {
                    return (false, subjectValidation.errorMessage);
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return (false, yearValidation.errorMessage);
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return (false, semesterValidation.errorMessage);
                }

                var scoreValidation = ValidationHelper.ValidateScoreSet(qtScore, gkScore, ckScore);
                if (!scoreValidation.isValid)
                {
                    return (false, scoreValidation.errorMessage);
                }

                var student = _studentRepository.GetById(studentId);
                if (student == null)
                {
                    return (false, "Student not found.");
                }

                if (!CanCurrentUserAccessScores(student.ClassId, subjectId, schoolYear, semester))
                {
                    return (false, "You do not have permission to modify these scores.");
                }

                decimal? finalScore = CalculateFinalScore(qtScore, gkScore, ckScore);

                var score = new Score
                {
                    StudentId = studentId,
                    SubjectId = subjectId,
                    SchoolYear = schoolYear.Trim(),
                    Semester = semester,
                    QtScore = qtScore,
                    GkScore = gkScore,
                    CkScore = ckScore,
                    FnScore = finalScore
                };

                bool saved = _scoreRepository.UpsertScore(score);

                if (saved)
                {
                    return (true, "Score saved successfully.");
                }
                else
                {
                    return (false, "Failed to save score to database.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error saving score: {ex.Message}");
            }
        }

        public List<StudentScoreDTO> GetStudentScores(int studentId, string schoolYear, int semester)
        {
            try
            {
                var studentValidation = ValidationHelper.ValidateId(studentId, "Student");
                if (!studentValidation.isValid)
                {
                    return new List<StudentScoreDTO>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<StudentScoreDTO>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<StudentScoreDTO>();
                }

                if (!SessionManager.CanAccessStudent(studentId))
                {
                    return new List<StudentScoreDTO>();
                }

                var scores = _scoreRepository.GetStudentScores(studentId, schoolYear, semester);
                return scores?.ToList() ?? new List<StudentScoreDTO>();
            }
            catch (Exception ex)
            {
                return new List<StudentScoreDTO>();
            }
        }

        public (bool success, string message) DeleteScore(
            int studentId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (!SessionManager.HasRole(2))
                {
                    return (false, "Only teachers and administrators can delete scores.");
                }

                var student = _studentRepository.GetById(studentId);
                if (student == null)
                {
                    return (false, "Student not found.");
                }

                if (!CanCurrentUserAccessScores(student.ClassId, subjectId, schoolYear, semester))
                {
                    return (false, "You do not have permission to delete this score.");
                }

                bool deleted = _scoreRepository.DeleteScore(studentId, subjectId, schoolYear, semester);

                if (deleted)
                {
                    return (true, "Score deleted successfully.");
                }
                else
                {
                    return (false, "Failed to delete score. It may not exist.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting score: {ex.Message}");
            }
        }

        private bool ValidateScoreQueryParameters(
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

        private bool CanCurrentUserAccessScores(
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