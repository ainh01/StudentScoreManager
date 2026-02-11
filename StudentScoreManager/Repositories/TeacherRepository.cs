using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class TeacherRepository : IRepository<Teacher>
    {
        public IEnumerable<Teacher> GetAll()
        {
            var teachers = new List<Teacher>();
            string query = "SELECT id, name, sex, birth_year FROM teachers ORDER BY name";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add(new Teacher
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Sex = reader.IsDBNull(2) ? null : reader.GetChar(2),
                            BirthYear = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                        });
                    }
                }
            }
            return teachers;
        }

        public Teacher GetById(int id)
        {
            string query = "SELECT id, name, sex, birth_year FROM teachers WHERE id = @id";

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
                            return new Teacher
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Sex = reader.IsDBNull(2) ? null : reader.GetChar(2),
                                BirthYear = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool Insert(Teacher entity)
        {
            string query = "INSERT INTO teachers (name, sex, birth_year) VALUES (@name, @sex, @birthYear)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        cmd.Parameters.AddWithValue("@sex", entity.Sex.HasValue ? (object)entity.Sex.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@birthYear", entity.BirthYear.HasValue ? (object)entity.BirthYear.Value : DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting teacher: {ex.Message}");
                return false;
            }
        }

        public bool Update(Teacher entity)
        {
            string query = "UPDATE teachers SET name = @name, sex = @sex, birth_year = @birthYear WHERE id = @id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", entity.Id);
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        cmd.Parameters.AddWithValue("@sex", entity.Sex.HasValue ? (object)entity.Sex.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@birthYear", entity.BirthYear.HasValue ? (object)entity.BirthYear.Value : DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating teacher: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int id)
        {
            string query = "DELETE FROM teachers WHERE id = @id";

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
                System.Diagnostics.Debug.WriteLine($"Error deleting teacher: {ex.Message}");
                return false;
            }
        }
    }
}