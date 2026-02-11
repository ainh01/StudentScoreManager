namespace StudentScoreManager.Models.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public char? Sex { get; set; }

        public int? BirthYear { get; set; }

        public int? Age => BirthYear.HasValue ? System.DateTime.Now.Year - BirthYear.Value : null;

        public string SexDisplay => Sex.HasValue ? (Sex.Value == 'M' ? "Male" : "Female") : "Not Specified";
    }
}
