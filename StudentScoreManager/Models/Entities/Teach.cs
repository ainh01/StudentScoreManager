namespace StudentScoreManager.Models.Entities
{
    public class Teach
    {
        public int ClassId { get; set; }

        public int SubjectId { get; set; }

        public string SchoolYear { get; set; }

        public int Semester { get; set; }

        public int TeacherId { get; set; }
    }
}