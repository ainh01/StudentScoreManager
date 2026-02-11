using System;
using System.Collections.Generic;
using Npgsql;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Repositories
{
    public class ClassRepository : IRepository<Class>
    {
        public IEnumerable<Class> GetAll()
        {
            var classes = new List<Class>();
            string query = "SELECT id, name, grade_level, school_year FROM classes ORDER BY school_year DESC, grade_level, name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            classes.Add(new Class
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                GradeLevel = reader.GetInt32(2),
                                SchoolYear = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all classes: {ex.Message}");
            }
            return classes;
        }

        public Class GetById(int id)
        {
            string query = "SELECT id, name, grade_level, school_year FROM classes WHERE id = @id";
            Class classObj = null;

            try
            {
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
                                classObj = new Class
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    GradeLevel = reader.GetInt32(2),
                                    SchoolYear = reader.GetString(3)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting class by ID {id}: {ex.Message}");
            }
            return classObj;
        }

        public IEnumerable<Class> GetBySchoolYear(string schoolYear)
        {
            var classes = new List<Class>();
            string query = "SELECT id, name, grade_level, school_year FROM classes WHERE school_year = @schoolYear ORDER BY grade_level, name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                classes.Add(new Class
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    GradeLevel = reader.GetInt32(2),
                                    SchoolYear = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting classes by school year '{schoolYear}': {ex.Message}");
            }
            return classes;
        }

        public IEnumerable<Class> GetByTeacherId(int teacherId, string schoolYear, int semester)
        {
            var classes = new List<Class>();
            string query = @"  
        SELECT DISTINCT c.id, c.name, c.grade_level, c.school_year  
        FROM classes c  
        INNER JOIN teach t ON c.id = t.class_id  
        WHERE t.teacher_id = @teacherId  
          AND t.school_year = @schoolYear  
          AND t.semester = @semester  
        ORDER BY c.grade_level, c.name";

            try
            {
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
                                classes.Add(new Class
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    GradeLevel = reader.GetInt32(2),
                                    SchoolYear = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting classes by teacher ID {teacherId}, year {schoolYear}, semester {semester}: {ex.Message}");
            }
            return classes;
        }
        public IEnumerable<Class> GetByTeacherIdAndSchoolYearSemester(int teacherId, string schoolYear, int semester)
        {
            var classes = new List<Class>();
            string query = @"  
                SELECT DISTINCT c.id, c.name, c.grade_level, c.school_year  
                FROM classes c  
                INNER JOIN teach t ON c.id = t.class_id  
                WHERE t.teacher_id = @teacherId  
                  AND t.school_year = @schoolYear  
                  AND t.semester = @semester  
                ORDER BY c.grade_level, c.name";

            try
            {
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
                                classes.Add(new Class
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    GradeLevel = reader.GetInt32(2),
                                    SchoolYear = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ObjectDisposedException in GetByTeacherIdAndSchoolYearSemester: {ex.Message}");
                throw;
            }
            catch (NpgsqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in GetByTeacherIdAndSchoolYearSemester (Teacher ID: {teacherId}, Year: {schoolYear}, Semester: {semester}): {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An unexpected error occurred in GetByTeacherIdAndSchoolYearSemester (Teacher ID: {teacherId}, Year: {schoolYear}, Semester: {semester}): {ex.Message}");
            }
            return classes;
        }

        public IEnumerable<Class> GetAllBySchoolYearSemester(string schoolYear, int semester)
        {
            var classes = new List<Class>();
            string query = @"  
                SELECT DISTINCT c.id, c.name, c.grade_level, c.school_year  
                FROM classes c  
                INNER JOIN teach t ON c.id = t.class_id  
                WHERE t.school_year = @schoolYear  
                  AND t.semester = @semester  
                ORDER BY c.grade_level, c.name";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                classes.Add(new Class
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    GradeLevel = reader.GetInt32(2),
                                    SchoolYear = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all classes by school year '{schoolYear}' and semester '{semester}': {ex.Message}");
            }
            return classes;
        }

        public bool Insert(Class entity)
        {
            string query = @"  
                INSERT INTO classes (name, grade_level, school_year)  
                VALUES (@name, @gradeLevel, @schoolYear)";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", entity.Name);
                        cmd.Parameters.AddWithValue("@gradeLevel", entity.GradeLevel);
                        cmd.Parameters.AddWithValue("@schoolYear", entity.SchoolYear);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting class: {ex.Message}");
                return false;
            }
        }

        public bool Update(Class entity)
        {
            string query = @"  
                UPDATE classes  
                SET name = @name,  
                    grade_level = @gradeLevel,  
                    school_year = @schoolYear  
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
                        cmd.Parameters.AddWithValue("@gradeLevel", entity.GradeLevel);
                        cmd.Parameters.AddWithValue("@schoolYear", entity.SchoolYear);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating class: {ex.Message}");
                return false;
            }
        }

        public bool Delete(int id)
        {
            string query = "DELETE FROM classes WHERE id = @id";

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
                System.Diagnostics.Debug.WriteLine($"Error deleting class: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<string> GetSchoolYears()
        {
            var years = new List<string>();
            string query = "SELECT DISTINCT school_year FROM classes ORDER BY school_year DESC";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            years.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting school years: {ex.Message}");
            }
            return years;
        }
    }
}