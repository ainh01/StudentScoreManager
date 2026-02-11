using System;
using System.Collections.Generic;
using System.Linq;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Repositories;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Controllers
{
    public class SubjectController : IDisposable
    {
        private readonly SubjectRepository _subjectRepository;
        private bool _disposed = false;

        public SubjectController()
        {
            _subjectRepository = new SubjectRepository();
        }

        public List<Subject> GetAllSubjects()
        {
            try
            {
                var subjects = _subjectRepository.GetAll();
                return subjects?.ToList() ?? new List<Subject>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<Subject>();
            }
        }

        public List<dynamic> GetAllSubjects(int classId, string schoolYear, int semester)
        {
            try
            {
                var classValidation = ValidationHelper.ValidateId(classId, "Class");
                if (!classValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var subjects = _subjectRepository.GetByClassYearSemester(classId, schoolYear, semester);
                return subjects?.Select(s => new
                {
                    SubjectId = s.Id,
                    Name = s.Name
                }).Cast<dynamic>().ToList() ?? new List<dynamic>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<dynamic>();
            }
        }

        public List<dynamic> GetSubjectsForTeacher(int teacherId, int classId, string schoolYear, int semester)
        {
            try
            {
                var teacherValidation = ValidationHelper.ValidateId(teacherId, "Teacher");
                if (!teacherValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var classValidation = ValidationHelper.ValidateId(classId, "Class");
                if (!classValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<dynamic>();
                }

                var subjects = _subjectRepository.GetByTeacher(teacherId, classId, schoolYear, semester);
                return subjects?.Select(s => new
                {
                    SubjectId = s.Id,
                    Name = s.Name
                }).Cast<dynamic>().ToList() ?? new List<dynamic>();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<dynamic>();
            }
        }

        public Subject? GetSubjectById(int subjectId)
        {
            try
            {
                var validation = ValidationHelper.ValidateId(subjectId, "Subject");
                if (!validation.isValid)
                {
                    return null;
                }

                return _subjectRepository.GetById(subjectId);
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

        public List<Subject> GetSubjectsForCurrentUser(int classId, string schoolYear, int semester)
        {
            try
            {
                var classValidation = ValidationHelper.ValidateId(classId, "Class");
                if (!classValidation.isValid)
                {
                    return new List<Subject>();
                }

                var yearValidation = ValidationHelper.ValidateSchoolYear(schoolYear);
                if (!yearValidation.isValid)
                {
                    return new List<Subject>();
                }

                var semesterValidation = ValidationHelper.ValidateSemester(semester);
                if (!semesterValidation.isValid)
                {
                    return new List<Subject>();
                }

                if (SessionManager.IsAdmin() || SessionManager.IsStudent())
                {
                    var subjects = _subjectRepository.GetByClassYearSemester(
                        classId, schoolYear, semester);
                    return subjects?.ToList() ?? new List<Subject>();
                }

                if (SessionManager.IsTeacher())
                {
                    int? teacherId = SessionManager.GetTeacherId();
                    if (!teacherId.HasValue)
                    {
                        return new List<Subject>();
                    }

                    var subjects = _subjectRepository.GetByTeacher(
                        teacherId.Value, classId, schoolYear, semester);
                    return subjects?.ToList() ?? new List<Subject>();
                }

                return new List<Subject>();
            }
            catch (Exception ex)
            {
                return new List<Subject>();
            }
        }

        public (bool success, string message) CreateSubject(string name)
        {
            try
            {
                if (!SessionManager.IsAdmin())
                {
                    return (false, "Only administrators can create subjects.");
                }

                var nameValidation = ValidationHelper.ValidateRequiredString(name, "Subject name");
                if (!nameValidation.isValid)
                {
                    return (false, nameValidation.errorMessage);
                }

                var newSubject = new Subject
                {
                    Name = name.Trim()
                };

                bool inserted = _subjectRepository.Insert(newSubject);

                if (inserted)
                {
                    return (true, "Subject created successfully.");
                }
                else
                {
                    return (false, "Failed to create subject in database.");
                }
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return (false, $"Error creating subject: {ex.Message}");
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
                    if (_subjectRepository is IDisposable repoDisposable)
                    {
                        repoDisposable.Dispose();
                    }
                }

                _disposed = true;
            }
        }
    }
}