using System;

namespace StudentScoreManager.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public int Role { get; set; }

        public int? LinkedId { get; set; }

        public string LinkedType { get; set; }

        public string RoleName => Role switch
        {
            1 => "Học Sinh",
            2 => "Giáo Viên",
            3 => "Admin",
            _ => "Không Rõ"
        };

        public bool HasRole(int requiredRole)
        {
            return Role >= requiredRole;
        }
    }
}
