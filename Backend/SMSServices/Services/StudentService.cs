using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.CombineModel;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.Repository;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper mapper;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, IMapper mapper, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            this.mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Student>> GetAllStudentAsync(Guid schoolId)
        {
            _logger.LogInformation("Fetching all students for school {SchoolId}", schoolId);
            var students = await _studentRepository.GetAllStudentAsync(schoolId);
            _logger.LogInformation("Retrieved {Count} students for school {SchoolId}", students.Count(), schoolId);
            return students;
        }

        public async Task<PagedResult<Student>> GetAllStudentPagedAsync(Guid schoolId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching students (paginated) for school {SchoolId}, page {PageNumber}, size {PageSize}", 
                schoolId, pageNumber, pageSize);
            var result = await _studentRepository.GetAllStudentPagedAsync(schoolId, pageNumber, pageSize);
            _logger.LogInformation("Retrieved page {PageNumber} with {Count} students (total: {TotalCount}) for school {SchoolId}", 
                pageNumber, result.Items.Count(), result.TotalCount, schoolId);
            return result;
        }


        public async Task<Student> GetStudentByIdAsync(Guid studentId)
        {
            _logger.LogInformation("Fetching student with ID {StudentId}", studentId);
            var result = await _studentRepository.GetStudentByIdAsync(studentId);
            if (result != null)
            {
                _logger.LogInformation("Student {StudentId} found: {FirstName} {LastName}", 
                    studentId, result.FirstName, result.LastName);
                return result;
            }
            _logger.LogWarning("Student {StudentId} not found", studentId);
            throw new Exception("Student not found");
        }

        
        public async Task<IEnumerable<Student>> GetStudentByClassIdAsync(Guid classtId)
        {
            _logger.LogInformation("Fetching students for class {ClassId}", classtId);
            var result = await _studentRepository.GetStudentByClassIdAsync(classtId);
            if (result != null)
            {
                _logger.LogInformation("Retrieved {Count} students for class {ClassId}", result.Count(), classtId);
                return result;
            }
            _logger.LogWarning("No students found for class {ClassId}", classtId);
            throw new Exception("Students with class Id not found");
        }

        public async Task<PagedResult<Student>> GetStudentByClassIdPagedAsync(Guid classId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching students (paginated) for class {ClassId}, page {PageNumber}, size {PageSize}", 
                classId, pageNumber, pageSize);
            var result = await _studentRepository.GetStudentByClassIdPagedAsync(classId, pageNumber, pageSize);
            _logger.LogInformation("Retrieved page {PageNumber} with {Count} students for class {ClassId}", 
                pageNumber, result.Items.Count(), classId);
            return result;
        }


        public async Task<Student> CreateStudentAsync(CreateStudentRqstDto createStudent)
        {
            _logger.LogInformation("Creating new student: {FirstName} {LastName}, Email: {Email}", 
                createStudent.FirstName, createStudent.LastName, createStudent.Email);
            var newStudent = mapper.Map<Student>(createStudent);
            var result = await _studentRepository.CreateStudentAsync(newStudent);
            _logger.LogInformation("Student created successfully with ID {StudentId}", result.Id);
            return result;
        }

        public async Task<Student> UpdateStudentAsync(Guid id, UpdateStudentRequestDto updateStudentRequestDto)
        {
            _logger.LogInformation("Updating student {StudentId}", id);
            var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
            if (existingStudent != null)
            {
                existingStudent = mapper.Map(updateStudentRequestDto, existingStudent);
                var result = await _studentRepository.UpdateStudentAsync(existingStudent);
                _logger.LogInformation("Student {StudentId} updated successfully", id);
                return result;
            }
            _logger.LogWarning("Cannot update student {StudentId} - not found", id);
            throw new Exception("Student with this Id not found");
        }
        public async Task<Student> DeleteStudentAsync(Guid id)
        {
            _logger.LogInformation("Deleting student {StudentId}", id);
            var student = await _studentRepository.GetStudentByIdAsync(id);
            if (student != null)
            {
                var result = await _studentRepository.DeleteStudentAsync(student);
                _logger.LogInformation("Student {StudentId} ({FirstName} {LastName}) deleted successfully", 
                    id, student.FirstName, student.LastName);
                return result;
            }
            _logger.LogWarning("Cannot delete student {StudentId} - not found", id);
            throw new Exception("Student with this ID not found");
        }
    }
}
