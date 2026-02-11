using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class UserRepository : IRepository<User>
    {
        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();
            string query = "SELECT id, username, role, linked_id, linked_type FROM users ORDER BY id";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetInt32(2),
                            LinkedId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            LinkedType = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }
            }
            return users;
        }

        public User GetById(int id)
        {
            string query = "SELECT id, username, role, linked_id, linked_type FROM users WHERE id = @id";

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
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Role = reader.GetInt32(2),
                                LinkedId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                LinkedType = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public User GetByUsername(string username)
        {
            string query = "SELECT id, username, password_hash, role, linked_id, linked_type FROM users WHERE username = @username";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetInt32(3),
                                LinkedId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                                LinkedType = reader.IsDBNull(5) ? null : reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public LoginResponseDTO AuthenticateUser(string username, string passwordHash)
        {
            string query = @"  
        SELECT   
            u.id,   
            u.username,   
            u.role,   
            u.linked_id,   
            u.linked_type,  
            CASE   
                WHEN u.linked_type = 'student' THEN s.name  
                WHEN u.linked_type = 'teacher' THEN t.name  
                ELSE 'Administrator'  
            END AS display_name  
        FROM users u  
        LEFT JOIN students s ON u.linked_type = 'student' AND u.linked_id = s.id  
        LEFT JOIN teachers t ON u.linked_type = 'teacher' AND u.linked_id = t.id  
        WHERE u.username = @username";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int role = reader.GetInt32(2);
                            return new LoginResponseDTO
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Role = role,
                                RoleName = role == 1 ? "Student" : role == 2 ? "Teacher" : "Admin",
                                LinkedId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                LinkedType = reader.IsDBNull(4) ? null : reader.GetString(4),
                                DisplayName = reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool Insert(User entity)
        {
            string query = @"  
                INSERT INTO users (username, password_hash, role, linked_id, linked_type)  
                VALUES (@username, @passwordHash, @role, @linkedId, @linkedType)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", entity.Username);
                        cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);
                        cmd.Parameters.AddWithValue("@role", entity.Role);
                        cmd.Parameters.AddWithValue("@linkedId", entity.LinkedId.HasValue ? (object)entity.LinkedId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@linkedType", entity.LinkedType ?? (object)DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting user: {ex.Message}");
                return false;
            }
        }

        public bool Update(User entity)
        {
            string query = @"  
                UPDATE users   
                SET username = @username,   
                    password_hash = @passwordHash,   
                    role = @role,   
                    linked_id = @linkedId,   
                    linked_type = @linkedType  
                WHERE id = @id";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", entity.Id);
                        cmd.Parameters.AddWithValue("@username", entity.Username);
                        cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);
                        cmd.Parameters.AddWithValue("@role", entity.Role);
                        cmd.Parameters.AddWithValue("@linkedId", entity.LinkedId.HasValue ? (object)entity.LinkedId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@linkedType", entity.LinkedType ?? (object)DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int id)
        {
            string query = "DELETE FROM users WHERE id = @id";

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
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        public bool UsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM users WHERE username = @username";

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}