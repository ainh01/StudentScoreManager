namespace StudentScoreManager.Models.DTOs
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int Role { get; set; }

        public int? LinkedId { get; set; }

        public string? LinkedType { get; set; }

        public string? DisplayName { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}
