using System.ComponentModel.DataAnnotations;

namespace StudentScoreManager.Models.Entities
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Subject name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string Category => (Name ?? string.Empty) switch
        {
            "Toán" or "Vật Lý" or "Hóa Học" or "Sinh Học" => "STEM",
            "Ngữ Văn" or "Lịch Sử" or "Địa Lý" => "Social Sciences",
            "Tiếng Anh" => "Foreign Language",
            "" => "Uncategorized",
            _ => "Other"
        };
    }
}