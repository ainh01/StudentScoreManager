using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class SubjectRepository : IRepository<Subject>
    {
        public IEnumerable<Subject> GetAll()
        {
            var subjects = new List<Subject>();
            string query = "SELECT id, name FROM subjects ORDER BY name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subjects.Add(new Subject
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return subjects;
        }

        public Subject GetById(int id)
        {
            string query = "SELECT id, name FROM subjects WHERE id = @id";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Subject
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Subject> GetByTeacher(int teacherId, int classId, string schoolYear, int semester)
        {
            var subjects = new List<Subject>();
            string query = @"  
                SELECT DISTINCT s.id, s.name  
                FROM subjects s  
                INNER JOIN teach t ON s.id = t.subject_id  
                WHERE t.teacher_id = @teacherId  
                  AND t.class_id = @classId  
                  AND t.school_year = @schoolYear  
                  AND t.semester = @semester  
                ORDER BY s.name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@classId", classId);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new Subject
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return subjects;
        }

        public IEnumerable<Subject> GetByClassYearSemester(int classId, string schoolYear, int semester)
        {
            var subjects = new List<Subject>();
            string query = @"  
                SELECT DISTINCT s.id, s.name  
                FROM subjects s  
                INNER JOIN teach t ON s.id = t.subject_id  
                WHERE t.class_id = @classId  
                  AND t.school_year = @schoolYear  
                  AND t.semester = @semester  
                ORDER BY s.name";

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
                            subjects.Add(new Subject
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return subjects;
        }

        public bool Insert(Subject entity)
        {
            string query = "INSERT INTO subjects (name) VALUES (@name)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting subject: {ex.Message}");
                return false;
            }
        }

        public bool Update(Subject entity)
        {
            string query = "UPDATE subjects SET name = @name WHERE id = @id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", entity.Id);
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating subject: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int id)
        {
            string query = "DELETE FROM subjects WHERE id = @id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting subject: {ex.Message}");
                return false;
            }
        }
    }
}
