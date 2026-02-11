using System;
using StudentScoreManager.Models.DTOs;

namespace StudentScoreManager.Utils
{
    public static class SessionManager
    {
        private static int _userId;
        private static string _username;
        private static int _role;
        private static int? _linkedId;
        private static string _linkedType;
        private static string _displayName;
        private static bool _isAuthenticated;

        public static int UserId => _userId;

        public static int CurrentUserId => _userId;

        public static string Username => _username;

        public static int Role => _role;

        public static string RoleName => Role switch
        {
            1 => "Học Sinh",
            2 => "Giáo Viên",
            3 => "Admin",
            _ => "Không Rõ"
        };

        public static int? LinkedId => _linkedId;

        public static string LinkedType => _linkedType;

        public static string DisplayName => _displayName;

        public static bool IsAuthenticated => _isAuthenticated;

        public static void InitializeSession(LoginResponseDTO loginResponse)
        {
            if (loginResponse == null)
            {
                throw new ArgumentNullException(nameof(loginResponse),
                    "Cannot initialize session with null login response.");
            }

            _userId = loginResponse.UserId;
            _username = loginResponse.Username;
            _role = loginResponse.Role;
            _linkedId = loginResponse.LinkedId;
            _linkedType = loginResponse.LinkedType;
            _displayName = loginResponse.DisplayName ?? loginResponse.Username;
            _isAuthenticated = true;
        }

        public static void ClearSession()
        {
            _userId = 0;
            _username = null;
            _role = 0;
            _linkedId = null;
            _linkedType = null;
            _displayName = null;
            _isAuthenticated = false;
        }

        public static bool IsStudent() => _isAuthenticated && _role == 1;

        public static bool IsTeacher() => _isAuthenticated && _role == 2;

        public static bool IsAdmin() => _isAuthenticated && _role == 3;

        public static bool HasRole(int minimumRole)
        {
            return _isAuthenticated && _role >= minimumRole;
        }

        public static bool CanAccessStudent(int studentId)
        {
            if (!_isAuthenticated) return false;

            if (_role >= 2) return true;

            if (_role == 1 && _linkedType == "student")
            {
                return _linkedId == studentId;
            }

            return false;
        }

        public static int? GetStudentId()
        {
            if (_isAuthenticated && _role == 1 && _linkedType == "student")
            {
                return _linkedId;
            }
            return null;
        }

        public static int? GetTeacherId()
        {
            if (_isAuthenticated && _role == 2 && _linkedType == "teacher")
            {
                return _linkedId;
            }
            return null;
        }
    }
}
