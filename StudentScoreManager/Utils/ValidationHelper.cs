using System;
using System.Text.RegularExpressions;

namespace StudentScoreManager.Utils
{
    public static class ValidationHelper
    {
        public static (bool isValid, string errorMessage) ValidateId(int id, string entityName)
        {
            if (id <= 0)
            {
                return (false, $"{entityName} ID must be a positive number. Received: {id}");
            }
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateRequiredString(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (false, $"{fieldName} cannot be empty.");
            }
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateSchoolYear(string? schoolYear)
        {
            if (string.IsNullOrWhiteSpace(schoolYear))
            {
                return (false, "School year cannot be empty.");
            }

            var regex = new Regex(@"^(\d{4})-(\d{4})$");
            var match = regex.Match(schoolYear);

            if (!match.Success)
            {
                return (false, "School year must be in format YYYY-YYYY (e.g., 2023-2024).");
            }

            if (!int.TryParse(match.Groups[1].Value, out int startYear) ||
                !int.TryParse(match.Groups[2].Value, out int endYear))
            {
                return (false, "Invalid year values in school year.");
            }

            if (endYear != startYear + 1)
            {
                return (false, "School year end must be exactly one year after start (e.g., 2023-2024).");
            }

            if (startYear < 2000 || startYear > 2100)
            {
                return (false, "School year must be between 2000 and 2100.");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateGradeLevel(int gradeLevel)
        {
            if (gradeLevel < 1 || gradeLevel > 12)
            {
                return (false, $"Grade level must be between 1 and 12. Received: {gradeLevel}");
            }
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateSemester(int semester)
        {
            if (semester != 1 && semester != 2)
            {
                return (false, $"Semester must be 1 or 2. Received: {semester}");
            }
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateScore(decimal? score, bool allowNull = true)
        {
            if (score == null)
            {
                return allowNull
                    ? (true, string.Empty)
                    : (false, "Score cannot be null.");
            }

            if (score < 0 || score > 10)
            {
                return (false, $"Score must be between 0 and 10. Received: {score}");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateScoreSet(decimal? qtScore, decimal? gkScore, decimal? ckScore)
        {
            if (qtScore != null || gkScore != null || ckScore != null)
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, "At least one score (QtScore, GkScore, or CkScore) must be provided.");
            }
        }

        public static (bool isValid, string errorMessage) ValidateUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return (false, "Username cannot be empty.");
            }

            if (username.Length < 3 || username.Length > 20)
            {
                return (false, "Username must be between 3 and 20 characters long.");
            }

            var regex = new Regex(@"^[a-zA-Z0-9._-]+$");
            if (!regex.IsMatch(username))
            {
                return (false, "Username can only contain alphanumeric characters, periods (.), underscores (_), and hyphens (-).");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password cannot be empty.");
            }

            if (password.Length < 8 || password.Length > 30)
            {
                return (false, "Password must be between 8 and 30 characters long.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter.");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter.");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return (false, "Password must contain at least one digit.");
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=  
$  
{  
$  
};:<>|./?,-]"))
            {
                return (false, "Password must contain at least one special character (e.g., !@#$%^&*).");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateRole(int role)
        {
            const int ROLE_STUDENT = 1;
            const int ROLE_TEACHER = 2;
            const int ROLE_ADMIN = 3;

            if (role == ROLE_STUDENT || role == ROLE_TEACHER || role == ROLE_ADMIN)
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, $"Invalid role ID. Role must be one of: {ROLE_STUDENT} (Student), {ROLE_TEACHER} (Teacher), {ROLE_ADMIN} (Admin). Received: {role}");
            }
        }
    }
}