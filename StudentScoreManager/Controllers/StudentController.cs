using System;
using System.Collections.Generic;
using System.Linq;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class StudentController
    {
        private readonly StudentRepository _studentRepository;

        public StudentController()
        {
            _studentRepository = new StudentRepository();
        }

        public List<Student> GetStudentsByClass(int classId)
        {
            try
            {
                var validation = ValidationHelper.ValidateId(classId, "Class");
                if (!validation.isValid)
                {
                    return new List<Student>();
                }

                var students = _studentRepository.GetByClassId(classId);
                return students?.ToList() ?? new List<Student>();
            }
            catch (Exception ex)
            {
                return new List<Student>();
            }
        }

        public Student GetStudentById(int studentId)
        {
            try
            {
                var validation = ValidationHelper.ValidateId(studentId, "Student");
                if (!validation.isValid)
                {
                    return null;
                }

                if (!SessionManager.CanAccessStudent(studentId))
                {
                    return null;
                }

                return _studentRepository.GetById(studentId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Student> SearchStudents(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<Student>();
                }

                if (!SessionManager.HasRole(2))
                {
                    return new List<Student>();
                }

                var students = _studentRepository.SearchByName(searchTerm.Trim());
                return students?.ToList() ?? new List<Student>();
            }
            catch (Exception ex)
            {
                return new List<Student>();
            }
        }

        public List<Student> GetStudentsBySchoolYear(string schoolYear)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(schoolYear))
                {
                    return new List<Student>();
                }

                if (!SessionManager.IsAdmin())
                {
                    return new List<Student>();
                }

                var classController = new ClassController();
                var allClasses = new List<Class>();

                var semester1Classes = classController.GetClassesForCurrentUser(schoolYear, 1);
                var semester2Classes = classController.GetClassesForCurrentUser(schoolYear, 2);

                if (semester1Classes != null) allClasses.AddRange(semester1Classes);
                if (semester2Classes != null) allClasses.AddRange(semester2Classes);

                var uniqueClasses = allClasses.GroupBy(c => c.Id).Select(g => g.First()).ToList();

                if (!uniqueClasses.Any())
                {
                    return new List<Student>();
                }

                var allStudents = new List<Student>();
                foreach (var cls in uniqueClasses)
                {
                    var studentsInClass = _studentRepository.GetByClassId(cls.Id);
                    if (studentsInClass != null)
                    {
                        allStudents.AddRange(studentsInClass);
                    }
                }

                var uniqueStudents = allStudents.GroupBy(s => s.Id).Select(g => g.First()).ToList();

                return uniqueStudents;
            }
            catch (Exception ex)
            {
                return new List<Student>();
            }
        }


        public Student GetCurrentStudent()
        {
            try
            {
                if (!SessionManager.IsStudent())
                {
                    return null;
                }

                int? studentId = SessionManager.GetStudentId();
                if (!studentId.HasValue)
                {
                    return null;
                }

                return _studentRepository.GetById(studentId.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public (bool success, string message) CreateStudent(
            string name, DateTime birthday, char sex, int classId)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can create student records.");
                }

                var nameValidation = ValidationHelper.ValidateRequiredString(name, "Student name");
                if (!nameValidation.isValid)
                {
                    return (false, nameValidation.errorMessage);
                }

                var classValidation = ValidationHelper.ValidateId(classId, "Class");
                if (!classValidation.isValid)
                {
                    return (false, classValidation.errorMessage);
                }

                if (birthday >= DateTime.Now)
                {
                    return (false, "Date of birth must be in the past.");
                }

                int age = DateTime.Now.Year - birthday.Year;
                if (birthday > DateTime.Now.AddYears(-age)) age--;

                if (age > 25 || age < 5)
                {
                    return (false, "Invalid date of birth for a K-12 student (age must be between 5 and 25).");
                }

                char upperSex = char.ToUpper(sex);
                if (upperSex != 'M' && upperSex != 'F')
                {
                    return (false, "Sex must be 'M' (Male) or 'F' (Female).");
                }

                var newStudent = new Student
                {
                    Name = name.Trim(),
                    Birthday = birthday,
                    Sex = upperSex,
                    ClassId = classId
                };

                bool inserted = _studentRepository.Insert(newStudent);

                if (inserted)
                {
                    return (true, "Student created successfully.");
                }
                else
                {
                    return (false, "Failed to create student in database.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error creating student: {ex.Message}");
            }
        }

        public (bool success, string message) UpdateStudent(Student student)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can update student records.");
                }

                if (student == null)
                {
                    return (false, "Student data cannot be null.");
                }

                var idValidation = ValidationHelper.ValidateId(student.Id, "Student");
                if (!idValidation.isValid)
                {
                    return (false, idValidation.errorMessage);
                }

                var nameValidation = ValidationHelper.ValidateRequiredString(student.Name, "Student name");
                if (!nameValidation.isValid)
                {
                    return (false, nameValidation.errorMessage);
                }

                char upperSex = char.ToUpper(student.Sex);
                if (upperSex != 'M' && upperSex != 'F')
                {
                    return (false, "Sex must be 'M' (Male) or 'F' (Female).");
                }

                student.Sex = upperSex;

                if (student.Birthday >= DateTime.Now)
                {
                    return (false, "Date of birth must be in the past.");
                }

                bool updated = _studentRepository.Update(student);

                if (updated)
                {
                    return (true, "Student updated successfully.");
                }
                else
                {
                    return (false, "Failed to update student in database.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error updating student: {ex.Message}");
            }
        }

        public (bool success, string message) DeleteStudent(int studentId)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can delete student records.");
                }

                var validation = ValidationHelper.ValidateId(studentId, "Student");
                if (!validation.isValid)
                {
                    return (false, validation.errorMessage);
                }

                bool deleted = _studentRepository.Delete(studentId);

                if (deleted)
                {
                    return (true, "Student deleted successfully.");
                }
                else
                {
                    return (false, "Failed to delete student. Student may not exist or has dependent records.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting student: {ex.Message}");
            }
        }
    }
}