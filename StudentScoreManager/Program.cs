using System;
using System.Windows.Forms;
using StudentScoreManager.Views;
using StudentScoreManager.Utils;
using BCrypt.Net;

namespace StudentScoreManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (!VerifyDatabaseConnection())
            {
                MessageBox.Show(
                    "Unable to connect to the database. Please verify:\n\n" +
                    "1. PostgreSQL service is running\n" +
                    "2. Database 'qldiem' exists\n" +
                    "3. Connection string is correct\n" +
                    "4. Network connectivity\n\n" +
                    "Application will now exit.",
                    "Database Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }

        private static bool VerifyDatabaseConnection()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        cmd.ExecuteScalar();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError("Database Connection Verification Failed", ex);
                return false;
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogError("Application Thread Exception", e.Exception);

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\n" +
                "The application will attempt to continue. If problems persist, please restart.",
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError("Unhandled Domain Exception", ex);
            }

            MessageBox.Show(
                "A critical error has occurred. The application must close.\n\n" +
                "Please contact technical support if this problem persists.",
                "Critical Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop);

            Environment.Exit(1);
        }

        private static void LogError(string context, Exception ex)
        {
            try
            {
                string logPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Logs");

                if (!System.IO.Directory.Exists(logPath))
                {
                    System.IO.Directory.CreateDirectory(logPath);
                }

                string logFile = System.IO.Path.Combine(
                    logPath,
                    $"ErrorLog_{DateTime.Now:yyyyMMdd}.txt");

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n" +
                                $"Exception: {ex.GetType().Name}\n" +
                                $"Message: {ex.Message}\n" +
                                $"Stack Trace: {ex.StackTrace}\n" +
                                $"{new string('-', 80)}\n";

                System.IO.File.AppendAllText(logFile, logEntry);
            }
            catch
            {
            }
        }
    }
}