using System;

namespace StudentScoreManager.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Birthday { get; set; }

        public char Sex { get; set; }

        public int ClassId { get; set; }

        public int Age => DateTime.Now.Year - Birthday.Year;

        public string SexDisplay => Sex == 'M' ? "Male" : "Female";
    }
}
