using System;
using System.Collections.Generic;
using System.Linq;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class ClassController : IDisposable
    {
        private readonly ClassRepository _classRepository;
        private readonly TeachRepository _teachRepository;
        private bool _disposed = false;

        public ClassController()
        {
            _classRepository = new ClassRepository();
            _teachRepository = new TeachRepository();
        }

        public List<Class> GetAllClassesUnfiltered()
        {
            try
            {
                var classes = _classRepository.GetAll();
                return classes?.ToList() ?? new List<Class>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Class>();
            }
        }

        public List<Class> GetAllClasses(string schoolYear, int semester)
        {
            try
            {
                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<Class>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                   return new List<Class>();
                }

                var classes = _classRepository.GetAllBySchoolYearSemester(schoolYear, semester);
                return classes?.ToList() ?? new List<Class>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Class>();
            }
        }

        public List<Class> GetClassesBySchoolYear(string schoolYear)
        {
            try
            {
                var validation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!validation.isValid)
                {
                    return new List<Class>();
                }

                var classes = _classRepository.GetBySchoolYear(schoolYear);
                return classes?.ToList() ?? new List<Class>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Class>();
            }
        }

        public List<Class> GetClassesForCurrentUser(string schoolYear, int semester)
        {
            try
            {
                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<Class>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<Class>();
                }

                if (SessionManager.IsAdmin())
                {
                    return GetAllClasses(schoolYear, semester);
                }

                if (SessionManager.IsTeacher())
                {
                    int? teacherId = SessionManager.GetTeacherId();
                    if (!teacherId.HasValue)
                    {
                        return new List<Class>();
                    }

                    var classes = _classRepository.GetByTeacherId(teacherId.Value, schoolYear, semester);
                    return classes?.ToList() ?? new List<Class>();
                }

                return new List<Class>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Class>();
            }
        }

        public List<Class> GetClassesForTeacher(int teacherId, string schoolYear, int semester)
        {
            try
            {
                var teacherIdValidation = ValidationHelper.ValidateId(teacherId, "Teacher ID");
                if (!teacherIdValidation.isValid)
                {
                    return new List<Class>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<Class>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<Class>();
                }

                var classes = _classRepository.GetByTeacherIdAndSchoolYearSemester(teacherId, schoolYear, semester);
                return classes?.ToList() ?? new List<Class>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Class>();
            }
        }

        public Class? GetClassById(int classId)
        {
            try
            {
                var validation = ValidationHelper.ValidateId(classId, "Class");
                if (!validation.isValid)
                {
                    return null;
                }

                return _classRepository.GetById(classId);
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetAllSchoolYears()
        {
            try
            {
                var schoolYears = _classRepository.GetSchoolYears();
                return schoolYears?.ToList() ?? new List<string>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        public (bool success, string message) CreateClass(string name, int gradeLevel, string schoolYear)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can create classes.");
                }

                var nameValidation = ValidationHelper.ValidateRequiredString(name, "Class name");
                if (!nameValidation.isValid)
                {
                    return (false, nameValidation.errorMessage);
                }

                var gradeValidation = ValidationHelper.ValidateGradeLevel(gradeLevel);
                if (!gradeValidation.isValid)
                {
                    return (false, gradeValidation.errorMessage);
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return (false, yearValidation.errorMessage);
                }

                var newClass = new Class
                {
                    Name = name.Trim(),
                    GradeLevel = gradeLevel,
                    SchoolYear = schoolYear.Trim()
                };

                bool inserted = _classRepository.Insert(newClass);

                if (inserted)
                {
                    return (true, "Class created successfully.");
                }
                else
                {
                    return (false, "Failed to create class in database.");
                }
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return (false, $"Error creating class: {ex.Message}");
            }
        }

        public bool CanCurrentUserAccessClass(int classId, int subjectId, string schoolYear, int semester)
        {
            try
            {
                if (SessionManager.IsAdmin())
                {
                    return true;
                }

                if (SessionManager.IsTeacher())
                {
                    int? teacherId = SessionManager.GetTeacherId();
                    if (!teacherId.HasValue)
                    {
                        return false;
                    }

                    return _teachRepository.IsTeacherAssigned(
                        teacherId.Value, classId, subjectId, schoolYear, semester);
                }

                return false;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_classRepository is IDisposable classRepoDisposable)
                    {
                        classRepoDisposable.Dispose();
                    }

                    if (_teachRepository is IDisposable teachRepoDisposable)
                    {
                        teachRepoDisposable.Dispose();
                    }
                }

                _disposed = true;
            }
        }
    }
}