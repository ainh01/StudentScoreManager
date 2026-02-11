using System;
using System.Configuration;
using System.Data;
using Npgsql;

namespace StudentScoreManager.Utils
{
    public static class DatabaseConnection
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"]?.ConnectionString;

                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = "Host=localhost;Port=5432;Database=qldiem;Username=postgres;Password=1704";
                        System.Diagnostics.Debug.WriteLine("WARNING: Using hardcoded connection string. Configure App.config for production.");
                    }
                }
                return _connectionString;
            }
        }

        public static NpgsqlConnection GetConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(ConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create database connection. Check connection string configuration.", ex);
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand("SELECT 1", connection))
                    {
                        var result = cmd.ExecuteScalar();
                        return result != null && (int)result == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        public static (bool isConnected, string serverVersion, string databaseName) GetConnectionInfo()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return (true, connection.PostgreSqlVersion.ToString(), connection.Database);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
