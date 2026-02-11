using System;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class AuthController
    {
        private readonly UserRepository _userRepository;

        public AuthController()
        {
            _userRepository = new UserRepository();
        }

        public LoginResponseDTO Login(string username, string plainPassword)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LOGIN START ===");

                var usernameValidation = ValidationHelper.ValidateUsername(username);
                if (!usernameValidation.isValid)
                {
                    System.Diagnostics.Debug.WriteLine("FAILED: Username validation");
                    return null;
                }
                System.Diagnostics.Debug.WriteLine("✓ Username validation passed");

                if (string.IsNullOrEmpty(plainPassword))
                {
                    System.Diagnostics.Debug.WriteLine("FAILED: Password is empty");
                    return null;
                }
                System.Diagnostics.Debug.WriteLine("✓ Password is not empty");

                User user = _userRepository.GetByUsername(username.Trim());
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("FAILED: User not found in database");
                    return null;
                }
                System.Diagnostics.Debug.WriteLine($"✓ User found - ID: {user.Id}, Username: {user.Username}");
                System.Diagnostics.Debug.WriteLine($"  Password hash length: {user.PasswordHash?.Length ?? 0}");

                bool isPasswordValid = PasswordHasher.VerifyPassword(plainPassword, user.PasswordHash);
                System.Diagnostics.Debug.WriteLine($"  Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    System.Diagnostics.Debug.WriteLine("FAILED: Password verification failed");
                    return null;
                }
                System.Diagnostics.Debug.WriteLine("✓ Password verified successfully");

                if (PasswordHasher.NeedsRehash(user.PasswordHash))
                {
                    System.Diagnostics.Debug.WriteLine("  Password needs rehashing...");
                    string newHash = PasswordHasher.HashPassword(plainPassword);
                    user.PasswordHash = newHash;
                    _userRepository.Update(user);
                    System.Diagnostics.Debug.WriteLine("  Password rehashed and updated");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  Password hash is current");
                }

                System.Diagnostics.Debug.WriteLine($"  Calling AuthenticateUser with username: {username}");
                System.Diagnostics.Debug.WriteLine($"  Using password hash (first 20 chars): {user.PasswordHash?.Substring(0, Math.Min(20, user.PasswordHash.Length))}...");

                LoginResponseDTO loginResponse = _userRepository.AuthenticateUser(username, user.PasswordHash);

                System.Diagnostics.Debug.WriteLine($"  AuthenticateUser returned: {loginResponse != null}");

                if (loginResponse != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ LoginResponse created - UserID: {loginResponse.UserId}, Role: {loginResponse.RoleName}");
                    SessionManager.InitializeSession(loginResponse);
                    System.Diagnostics.Debug.WriteLine("✓ Session initialized");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("FAILED: AuthenticateUser returned null");
                }

                System.Diagnostics.Debug.WriteLine("=== LOGIN END ===");
                return loginResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public void Logout()
        {
            SessionManager.ClearSession();
        }

        public (bool success, string message) ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                if (!SessionManager.IsAuthenticated)
                {
                    return (false, "You must be logged in to change your password.");
                }

                var passwordValidation = ValidationHelper.ValidatePassword(newPassword);
                if (!passwordValidation.isValid)
                {
                    return (false, passwordValidation.errorMessage);
                }

                User user = _userRepository.GetById(SessionManager.UserId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return (false, "Current password is incorrect.");
                }

                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                bool updated = _userRepository.Update(user);

                if (updated)
                {
                    return (true, "Password changed successfully.");
                }
                else
                {
                    return (false, "Failed to update password in database.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error changing password: {ex.Message}");
            }
        }

        public (bool success, string message) CreateUser(
            string username, string plainPassword, int role, int? linkedId, string linkedType)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can create user accounts.");
                }

                var usernameValidation = ValidationHelper.ValidateUsername(username);
                if (!usernameValidation.isValid)
                {
                    return (false, usernameValidation.errorMessage);
                }

                var passwordValidation = ValidationHelper.ValidatePassword(plainPassword);
                if (!passwordValidation.isValid)
                {
                    return (false, passwordValidation.errorMessage);
                }

                var roleValidation = ValidationHelper.ValidateRole(role);
                if (!roleValidation.isValid)
                {
                    return (false, roleValidation.errorMessage);
                }

                if (_userRepository.UsernameExists(username.Trim()))
                {
                    return (false, "Username already exists. Please choose a different username.");
                }

                if (role == 1 && linkedType != "student")
                {
                    return (false, "Student role must be linked to a student entity.");
                }

                if (role == 2 && linkedType != "teacher")
                {
                    return (false, "Teacher role must be linked to a teacher entity.");
                }

                if (role == 3 && (linkedId.HasValue || !string.IsNullOrEmpty(linkedType)))
                {
                    return (false, "Admin role should not have linked entities.");
                }

                var newUser = new User
                {
                    Username = username.Trim(),
                    PasswordHash = PasswordHasher.HashPassword(plainPassword),
                    Role = role,
                    LinkedId = linkedId,
                    LinkedType = linkedType
                };

                bool inserted = _userRepository.Insert(newUser);

                if (inserted)
                {
                    return (true, "User account created successfully.");
                }
                else
                {
                    return (false, "Failed to create user account in database.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error creating user: {ex.Message}");
            }
        }
    }
}
