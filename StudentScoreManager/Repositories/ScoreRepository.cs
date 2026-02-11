using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class ScoreRepository
    {
        private const decimal PASS_THRESHOLD = 5.0m;

        public IEnumerable<Score> GetByStudentId(int studentId)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("Student ID must be a positive integer.", nameof(studentId));
            }

            var scores = new List<Score>();
            string query = @"
                SELECT student_id, subject_id, school_year, semester, qt_score, gk_score, ck_score, fn_score
                FROM scores
                WHERE student_id = @studentId
                ORDER BY school_year DESC, semester DESC, subject_id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                scores.Add(MapReaderToScore(reader));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving scores for student ID {studentId}", ex);
                throw new InvalidOperationException($"Failed to retrieve scores for student ID {studentId}. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving scores for student ID {studentId}", ex);
                throw;
            }

            return scores;
        }

        public IEnumerable<ScoreSummaryDTO> GetScoreSummaryByClassAndSubject(int classId, int subjectId, string schoolYear, int semester)
        {
            ValidateClassSubjectParameters(classId, subjectId, schoolYear, semester);

            var scores = new List<ScoreSummaryDTO>();
            string query = @"
        SELECT
            s.id AS student_id,
            s.name AS student_name,
            sc.fn_score
        FROM students s
        LEFT JOIN scores sc ON s.id = sc.student_id
            AND sc.subject_id = @subjectId
            AND sc.school_year = @schoolYear
            AND sc.semester = @semester
        WHERE s.class_id = @classId
        ORDER BY s.name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.Parameters.AddWithValue("@subjectId", subjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                scores.Add(new ScoreSummaryDTO
                                {
                                    StudentId = reader.GetInt32(0),
                                    StudentName = reader.GetString(1),
                                    FnScore = reader.IsDBNull(2) ? null : reader.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving score summary for class {classId}, subject {subjectId}", ex);
                throw new InvalidOperationException("Failed to retrieve score summary. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving score summary for class {classId}, subject {subjectId}", ex);
                throw;
            }

            return scores;
        }

        public ScoreDetailDTO GetScoreDetail(int studentId, int subjectId, string schoolYear, int semester)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("Student ID must be a positive integer.", nameof(studentId));
            }
            ValidateClassSubjectParameters(0, subjectId, schoolYear, semester);

            string query = @"
                SELECT
                    s.id AS student_id,
                    s.name AS student_name,
                    sub.id AS subject_id,
                    sub.name AS subject_name,
                    sc.school_year,
                    sc.semester,
                    sc.qt_score,
                    sc.gk_score,
                    sc.ck_score,
                    sc.fn_score
                FROM students s
                CROSS JOIN subjects sub
                LEFT JOIN scores sc ON s.id = sc.student_id
                    AND sub.id = sc.subject_id
                    AND sc.school_year = @schoolYear
                    AND sc.semester = @semester
                WHERE s.id = @studentId AND sub.id = @subjectId";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        cmd.Parameters.AddWithValue("@subjectId", subjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ScoreDetailDTO
                                {
                                    StudentId = reader.GetInt32(0),
                                    StudentName = reader.GetString(1),
                                    SubjectId = reader.GetInt32(2),
                                    SubjectName = reader.GetString(3),
                                    SchoolYear = schoolYear,
                                    Semester = semester,
                                    QtScore = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                                    GkScore = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                                    CkScore = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                                    FnScore = reader.IsDBNull(9) ? null : reader.GetDecimal(9)
                                };
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving score detail for student {studentId}, subject {subjectId}", ex);
                throw new InvalidOperationException("Failed to retrieve score details. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving score detail for student {studentId}, subject {subjectId}", ex);
                throw;
            }

            return null;
        }

        public IEnumerable<StudentScoreDTO> GetStudentScores(int studentId, string schoolYear, int semester)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("Student ID must be a positive integer.", nameof(studentId));
            }
            ValidateSchoolYearAndSemester(schoolYear, semester);

            var scores = new List<StudentScoreDTO>();
            string query = @"
                SELECT
                    sub.name AS subject_name,
                    sc.qt_score,
                    sc.gk_score,
                    sc.ck_score,
                    sc.fn_score
                FROM subjects sub
                LEFT JOIN scores sc ON sub.id = sc.subject_id
                    AND sc.student_id = @studentId
                    AND sc.school_year = @schoolYear
                    AND sc.semester = @semester
                ORDER BY sub.name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                scores.Add(new StudentScoreDTO
                                {
                                    SubjectName = reader.GetString(0),
                                    QtScore = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                                    GkScore = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                                    CkScore = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                                    FnScore = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving student scores for student {studentId}", ex);
                throw new InvalidOperationException("Failed to retrieve student scores. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving student scores for student {studentId}", ex);
                throw;
            }

            return scores;
        }

        public IEnumerable<SubjectAverageDTO> GetSubjectAveragesByClass(int classId, string schoolYear, int semester)
        {
            if (classId <= 0)
            {
                throw new ArgumentException("Class ID must be a positive integer.", nameof(classId));
            }
            ValidateSchoolYearAndSemester(schoolYear, semester);

            var subjectAverages = new List<SubjectAverageDTO>();
            string query = @"
                SELECT
                    sub.name AS subject_name,
                    AVG(sc.fn_score) AS average_score
                FROM subjects sub
                LEFT JOIN scores sc ON sub.id = sc.subject_id
                    AND sc.school_year = @schoolYear
                    AND sc.semester = @semester
                LEFT JOIN students s ON sc.student_id = s.id
                    AND s.class_id = @classId
                WHERE sc.fn_score IS NOT NULL
                    AND s.id IS NOT NULL
                GROUP BY sub.id, sub.name
                ORDER BY sub.name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subjectAverages.Add(new SubjectAverageDTO
                                {
                                    SubjectName = reader.GetString(0),
                                    AverageScore = reader.IsDBNull(1) ? null : reader.GetDecimal(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving subject averages for class {classId}", ex);
                throw new InvalidOperationException($"Failed to retrieve subject averages for class {classId}. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving subject averages for class {classId}", ex);
                throw;
            }

            return subjectAverages;
        }

        public bool UpsertScore(Score score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score), "Score object cannot be null.");
            }

            if (score.StudentId <= 0)
            {
                throw new ArgumentException("Student ID must be a positive integer.", nameof(score.StudentId));
            }

            if (score.SubjectId <= 0)
            {
                throw new ArgumentException("Subject ID must be a positive integer.", nameof(score.SubjectId));
            }

            ValidateSchoolYearAndSemester(score.SchoolYear, score.Semester);
            ValidateScoreValues(score);

            string query = @"
                INSERT INTO scores (student_id, subject_id, school_year, semester, qt_score, gk_score, ck_score, fn_score)
                VALUES (@studentId, @subjectId, @schoolYear, @semester, @qtScore, @gkScore, @ckScore, @fnScore)
                ON CONFLICT (student_id, subject_id, school_year, semester)
                DO UPDATE SET
                    qt_score = EXCLUDED.qt_score,
                    gk_score = EXCLUDED.gk_score,
                    ck_score = EXCLUDED.ck_score,
                    fn_score = EXCLUDED.fn_score";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new NpgsqlCommand(query, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@studentId", score.StudentId);
                                cmd.Parameters.AddWithValue("@subjectId", score.SubjectId);
                                cmd.Parameters.AddWithValue("@schoolYear", score.SchoolYear);
                                cmd.Parameters.AddWithValue("@semester", score.Semester);
                                cmd.Parameters.AddWithValue("@qtScore", score.QtScore.HasValue ? (object)score.QtScore.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@gkScore", score.GkScore.HasValue ? (object)score.GkScore.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@ckScore", score.CkScore.HasValue ? (object)score.CkScore.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@fnScore", score.FnScore.HasValue ? (object)score.FnScore.Value : DBNull.Value);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                transaction.Commit();

                                return rowsAffected > 0;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error upserting score for student {score.StudentId}, subject {score.SubjectId}", ex);
                throw new InvalidOperationException("Failed to save score. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error upserting score for student {score.StudentId}, subject {score.SubjectId}", ex);
                throw;
            }
        }

        public bool DeleteScore(int studentId, int subjectId, string schoolYear, int semester)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("Student ID must be a positive integer.", nameof(studentId));
            }
            ValidateClassSubjectParameters(0, subjectId, schoolYear, semester);

            string query = @"
                DELETE FROM scores
                WHERE student_id = @studentId
                  AND subject_id = @subjectId
                  AND school_year = @schoolYear
                  AND semester = @semester";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new NpgsqlCommand(query, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@studentId", studentId);
                                cmd.Parameters.AddWithValue("@subjectId", subjectId);
                                cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                                cmd.Parameters.AddWithValue("@semester", semester);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                transaction.Commit();

                                return rowsAffected > 0;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error deleting score for student {studentId}, subject {subjectId}", ex);
                throw new InvalidOperationException("Failed to delete score. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error deleting score for student {studentId}, subject {subjectId}", ex);
                throw;
            }
        }

        public StatisticsDTO GetStatistics(int classId, int subjectId, string schoolYear, int semester)
        {
            ValidateClassSubjectParameters(classId, subjectId, schoolYear, semester);

            var allScores = new List<decimal>();
            StatisticsDTO stats = null;

            string query = @"
                SELECT
                    COUNT(s.id) AS total_students,
                    COUNT(sc.fn_score) AS graded_students,
                    AVG(sc.fn_score) AS average_score,
                    MIN(sc.fn_score) AS min_score,
                    MAX(sc.fn_score) AS max_score,
                    COUNT(CASE WHEN sc.fn_score >= @passThreshold THEN 1 END) AS pass_count,
                    COUNT(CASE WHEN sc.fn_score < @passThreshold THEN 1 END) AS fail_count
                FROM students s
                LEFT JOIN scores sc ON s.id = sc.student_id
                    AND sc.subject_id = @subjectId
                    AND sc.school_year = @schoolYear
                    AND sc.semester = @semester
                WHERE s.class_id = @classId";

            string scoresQuery = @"
                SELECT sc.fn_score
                FROM students s
                INNER JOIN scores sc ON s.id = sc.student_id
                WHERE s.class_id = @classId
                  AND sc.subject_id = @subjectId
                  AND sc.school_year = @schoolYear
                  AND sc.semester = @semester
                  AND sc.fn_score IS NOT NULL
                ORDER BY sc.fn_score";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.Parameters.AddWithValue("@subjectId", subjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);
                        cmd.Parameters.AddWithValue("@passThreshold", PASS_THRESHOLD);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats = new StatisticsDTO
                                {
                                    ClassId = classId,
                                    SubjectId = subjectId,
                                    TotalStudents = reader.GetInt32(reader.GetOrdinal("total_students")),
                                    GradedStudents = reader.GetInt32(reader.GetOrdinal("graded_students")),
                                    AverageScore = reader.IsDBNull(reader.GetOrdinal("average_score"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("average_score")),
                                    MinScore = reader.IsDBNull(reader.GetOrdinal("min_score"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("min_score")),
                                    MaxScore = reader.IsDBNull(reader.GetOrdinal("max_score"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("max_score")),
                                    PassCount = reader.GetInt32(reader.GetOrdinal("pass_count")),
                                    FailCount = reader.GetInt32(reader.GetOrdinal("fail_count"))
                                };
                            }
                        }
                    }

                    if (stats == null)
                    {
                        return null;
                    }

                    using (var cmd = new NpgsqlCommand(scoresQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.Parameters.AddWithValue("@subjectId", subjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allScores.Add(reader.GetDecimal(0));
                            }
                        }
                    }

                    stats.MedianScore = CalculateMedianFromList(allScores);
                    stats.StandardDeviation = CalculateStandardDeviationFromList(allScores);

                    return stats;
                }
            }
            catch (NpgsqlException ex)
            {
                LogError($"Database error retrieving statistics for class {classId}, subject {subjectId}", ex);
                throw new InvalidOperationException("Failed to retrieve statistics. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error retrieving statistics for class {classId}, subject {subjectId}", ex);
                throw;
            }
        }

        private decimal? CalculateMedianFromList(List<decimal> sortedScores)
        {
            if (sortedScores == null || sortedScores.Count == 0)
            {
                return null;
            }

            sortedScores.Sort();

            int count = sortedScores.Count;
            int middleIndex = count / 2;

            if (count % 2 == 0)
            {
                return (sortedScores[middleIndex - 1] + sortedScores[middleIndex]) / 2;
            }
            else
            {
                return sortedScores[middleIndex];
            }
        }

        private decimal? CalculateStandardDeviationFromList(List<decimal> scores)
        {
            if (scores == null || scores.Count == 0)
            {
                return null;
            }

            decimal average = scores.Average();
            decimal sumOfSquares = scores.Sum(score => (score - average) * (score - average));

            return (decimal)Math.Sqrt((double)(sumOfSquares / scores.Count));
        }

        [Obsolete("Use GetStatistics() which performs optimized single-connection calculation")]
        private decimal? CalculateMedian(int classId, int subjectId, string schoolYear, int semester)
        {
            var scores = FetchScoresForStatistics(classId, subjectId, schoolYear, semester);
            return CalculateMedianFromList(scores);
        }

        [Obsolete("Use GetStatistics() which performs optimized single-connection calculation")]
        private decimal? CalculateStandardDeviation(int classId, int subjectId, string schoolYear, int semester)
        {
            var scores = FetchScoresForStatistics(classId, subjectId, schoolYear, semester);
            return CalculateStandardDeviationFromList(scores);
        }

        private List<decimal> FetchScoresForStatistics(int classId, int subjectId, string schoolYear, int semester)
        {
            string query = @"
                SELECT sc.fn_score
                FROM students s
                INNER JOIN scores sc ON s.id = sc.student_id
                WHERE s.class_id = @classId
                  AND sc.subject_id = @subjectId
                  AND sc.school_year = @schoolYear
                  AND sc.semester = @semester
                  AND sc.fn_score IS NOT NULL
                ORDER BY sc.fn_score";

            var scores = new List<decimal>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            scores.Add(reader.GetDecimal(0));
                        }
                    }
                }
            }

            return scores;
        }

        private Score MapReaderToScore(NpgsqlDataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader), "Data reader cannot be null.");
            }

            try
            {
                return new Score
                {
                    StudentId = reader.GetInt32(0),
                    SubjectId = reader.GetInt32(1),
                    SchoolYear = reader.GetString(2),
                    Semester = reader.GetInt32(3),
                    QtScore = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    GkScore = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    CkScore = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                    FnScore = reader.IsDBNull(7) ? null : reader.GetDecimal(7)
                };
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new InvalidOperationException("Data reader column structure does not match expected Score entity structure.", ex);
            }
        }

        private void ValidateClassSubjectParameters(int classId, int subjectId, string schoolYear, int semester)
        {
            if (subjectId <= 0)
            {
                throw new ArgumentException("Subject ID must be a positive integer.", nameof(subjectId));
            }

            ValidateSchoolYearAndSemester(schoolYear, semester);
        }

        private void ValidateSchoolYearAndSemester(string schoolYear, int semester)
        {
            if (string.IsNullOrWhiteSpace(schoolYear))
            {
                throw new ArgumentException("School year cannot be null or empty.", nameof(schoolYear));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(schoolYear, @"^\d{4}-\d{4}$"))
            {
                throw new ArgumentException("School year must be in format 'YYYY-YYYY' (e.g., '2023-2024').", nameof(schoolYear));
            }

            if (semester < 1 || semester > 2)
            {
                throw new ArgumentException("Semester must be 1 or 2.", nameof(semester));
            }
        }

        private void ValidateScoreValues(Score score)
        {
            ValidateSingleScore(score.QtScore, "QT Score");
            ValidateSingleScore(score.GkScore, "GK Score");
            ValidateSingleScore(score.CkScore, "CK Score");
            ValidateSingleScore(score.FnScore, "Final Score");
        }

        private void ValidateSingleScore(decimal? scoreValue, string scoreName)
        {
            if (scoreValue.HasValue)
            {
                if (scoreValue.Value < 0 || scoreValue.Value > 10)
                {
                    throw new ArgumentException($"{scoreName} must be between 0 and 10. Received: {scoreValue.Value}", nameof(scoreValue));
                }
            }
        }

        private void LogError(string message, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            System.Diagnostics.Debug.WriteLine($"[ERROR] Exception: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
        }
    }
}
