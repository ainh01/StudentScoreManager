using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class StudentRepository : IRepository<Student>
    {
        public IEnumerable<Student> GetAll()
        {
            var students = new List<Student>();
            string query = "SELECT id, name, birthday, sex, class_id FROM students ORDER BY class_id, name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Birthday = reader.GetDateTime(2),
                            Sex = reader.GetChar(3),
                            ClassId = reader.GetInt32(4)
                        });
                    }
                }
            }
            return students;
        }

        public Student GetById(int id)
        {
            string query = "SELECT id, name, birthday, sex, class_id FROM students WHERE id = @id";

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
                            return new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Birthday = reader.GetDateTime(2),
                                Sex = reader.GetChar(3),
                                ClassId = reader.GetInt32(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Student> GetByClassId(int classId)
        {
            var students = new List<Student>();
            string query = "SELECT id, name, birthday, sex, class_id FROM students WHERE class_id = @classId ORDER BY name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Birthday = reader.GetDateTime(2),
                                Sex = reader.GetChar(3),
                                ClassId = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }
            return students;
        }

        public IEnumerable<Student> SearchByName(string searchTerm)
        {
            var students = new List<Student>();
            string query = @"
                SELECT id, name, birthday, sex, class_id 
                FROM students 
                WHERE LOWER(name) LIKE LOWER(@searchTerm)
                ORDER BY name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Birthday = reader.GetDateTime(2),
                                Sex = reader.GetChar(3),
                                ClassId = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }
            return students;
        }

        public bool Insert(Student entity)
        {
            string query = @"  
                INSERT INTO students (name, birthday, sex, class_id)  
                VALUES (@name, @birthday, @sex, @classId)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        cmd.Parameters.AddWithValue("@birthday", entity.Birthday);
                        cmd.Parameters.AddWithValue("@sex", entity.Sex);
                        cmd.Parameters.AddWithValue("@classId", entity.ClassId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting student: {ex.Message}");
                return false;
            }
        }

        public bool Update(Student entity)
        {
            string query = @"  
                UPDATE students   
                SET name = @name,   
                    birthday = @birthday,   
                    sex = @sex,   
                    class_id = @classId  
                WHERE id = @id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", entity.Id);
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        cmd.Parameters.AddWithValue("@birthday", entity.Birthday);
                        cmd.Parameters.AddWithValue("@sex", entity.Sex);
                        cmd.Parameters.AddWithValue("@classId", entity.ClassId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating student: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int id)
        {
            string query = "DELETE FROM students WHERE id = @id";

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
                System.Diagnostics.Debug.WriteLine($"Error deleting student: {ex.Message}");
                return false;
            }
        }

        public int GetStudentCountByClass(int classId)
        {
            string query = "SELECT COUNT(*) FROM students WHERE class_id = @classId";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
