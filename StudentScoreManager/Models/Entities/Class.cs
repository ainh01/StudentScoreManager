using System;
using System.ComponentModel.DataAnnotations;

namespace StudentScoreManager.Models.Entities
{
    public class Class
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Class name must be between 1 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "Grade level must be between 1 and 12")]
        public int GradeLevel { get; set; }

        [Required(ErrorMessage = "School year is required")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "School year must be in format YYYY-YYYY")]
        public string SchoolYear { get; set; } = string.Empty;

        public string DisplayName => $"{Name ?? "Unknown"} ({SchoolYear ?? "Unknown"})";

        public string GradeLevelName => GradeLevel switch
        {
            >= 1 and <= 5 => $"Elementary Grade {GradeLevel}",
            >= 6 and <= 9 => $"Middle School Grade {GradeLevel}",
            >= 10 and <= 12 => $"High School Grade {GradeLevel}",
            _ => $"Grade {GradeLevel} (Invalid)"
        };
    }
}