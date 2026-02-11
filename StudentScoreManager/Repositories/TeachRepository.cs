using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class TeachRepository
    {
        public IEnumerable<Teach> GetAll()
        {
            var assignments = new List<Teach>();
            string query = "SELECT class_id, subject_id, school_year, semester, teacher_id FROM teach ORDER BY school_year DESC, semester, class_id";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assignments.Add(new Teach
                        {
                            ClassId = reader.GetInt32(0),
                            SubjectId = reader.GetInt32(1),
                            SchoolYear = reader.GetString(2),
                            Semester = reader.GetInt32(3),
                            TeacherId = reader.GetInt32(4)
                        });
                    }
                }
            }
            return assignments;
        }

        public IEnumerable<Teach> GetByTeacherId(int teacherId, string schoolYear, int semester)
        {
            var assignments = new List<Teach>();
            string query = @"  
                SELECT class_id, subject_id, school_year, semester, teacher_id  
                FROM teach  
                WHERE teacher_id = @teacherId  
                  AND school_year = @schoolYear  
                  AND semester = @semester";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assignments.Add(new Teach
                            {
                                ClassId = reader.GetInt32(0),
                                SubjectId = reader.GetInt32(1),
                                SchoolYear = reader.GetString(2),
                                Semester = reader.GetInt32(3),
                                TeacherId = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }
            return assignments;
        }

        public bool Insert(Teach entity)
        {
            string query = @"  
                INSERT INTO teach (class_id, subject_id, school_year, semester, teacher_id)  
                VALUES (@classId, @subjectId, @schoolYear, @semester, @teacherId)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", entity.ClassId);
                        cmd.Parameters.AddWithValue("@subjectId", entity.SubjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", entity.SchoolYear);
                        cmd.Parameters.AddWithValue("@semester", entity.Semester);
                        cmd.Parameters.AddWithValue("@teacherId", entity.TeacherId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting teaching assignment: {ex.Message}");
                return false;
            }
        }

        public bool Update(Teach entity)
        {
            string query = @"  
                UPDATE teach   
                SET teacher_id = @teacherId  
                WHERE class_id = @classId   
                  AND subject_id = @subjectId   
                  AND school_year = @schoolYear   
                  AND semester = @semester";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@classId", entity.ClassId);
                        cmd.Parameters.AddWithValue("@subjectId", entity.SubjectId);
                        cmd.Parameters.AddWithValue("@schoolYear", entity.SchoolYear);
                        cmd.Parameters.AddWithValue("@semester", entity.Semester);
                        cmd.Parameters.AddWithValue("@teacherId", entity.TeacherId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating teaching assignment: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int classId, int subjectId, string schoolYear, int semester)
        {
            string query = @"  
                DELETE FROM teach   
                WHERE class_id = @classId   
                  AND subject_id = @subjectId   
                  AND school_year = @schoolYear   
                  AND semester = @semester";

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

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting teaching assignment: {ex.Message}");
                return false;
            }
        }

        public bool IsTeacherAssigned(int teacherId, int classId, int subjectId, string schoolYear, int semester)
        {
            string query = @"  
                SELECT COUNT(*)   
                FROM teach   
                WHERE teacher_id = @teacherId   
                  AND class_id = @classId   
                  AND subject_id = @subjectId   
                  AND school_year = @schoolYear   
                  AND semester = @semester";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@classId", classId);
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);

                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}