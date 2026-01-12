using AutoMapper;
using Microsoft.Extensions.Logging;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.CombineModel;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper mapper;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(ITeacherRepository teacherRepository, IMapper mapper, ILogger<TeacherService> logger)
        {
            _teacherRepository = teacherRepository;
            this.mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync(Guid schoolId)
        {
            _logger.LogInformation("Fetching all teachers for school {SchoolId}", schoolId);
            var teachers = await _teacherRepository.GetAllTeachersAsync(schoolId);
            _logger.LogInformation("Retrieved {Count} teachers for school {SchoolId}", teachers.Count(), schoolId);
            return teachers;
        }
        
        public async Task<PagedResult<Teacher>> GetAllTeachersPagedAsync(Guid schoolId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching teachers (paginated) for school {SchoolId}, page {PageNumber}, size {PageSize}", 
                schoolId, pageNumber, pageSize);
            var result = await _teacherRepository.GetAllTeachersPagedAsync(schoolId, pageNumber, pageSize);
            _logger.LogInformation("Retrieved page {PageNumber} with {Count} teachers (total: {TotalCount}) for school {SchoolId}", 
                pageNumber, result.Items.Count(), result.TotalCount, schoolId);
            return result;
        }
        
        public async Task<Teacher> GetTeacherByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching teacher with ID {TeacherId}", id);
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if (existingTeacher != null)
            {
                _logger.LogInformation("Teacher {TeacherId} found: {Name}", id, existingTeacher.Name);
                return existingTeacher;
            }
            _logger.LogWarning("Teacher {TeacherId} not found", id);
            throw new Exception("Id for this Teacher not found");
        }

        public async Task<Teacher> CreateTeacherAsync(CreateTeacherRqstDto teacherRqstDto)
        {
            _logger.LogInformation("Creating new teacher: {Name}, Email: {Email}", 
                teacherRqstDto.Name, teacherRqstDto.Email);
            var newTeacher = mapper.Map<Teacher>(teacherRqstDto);
            var result = await _teacherRepository.CreateTeacherAsync(newTeacher);
            _logger.LogInformation("Teacher created successfully with ID {TeacherId}", result.Id);
            return result;
        }

        public async Task<Teacher> UpdateTeacherAsync(Guid id, UpdateTeacherRequestDto updateTeacherRequestDto)
        {
            _logger.LogInformation("Updating teacher {TeacherId}", id);
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if(existingTeacher != null)
            {
                var teacher = mapper.Map(updateTeacherRequestDto, existingTeacher);
                var result = await _teacherRepository.UpdateTeacherAsync(teacher);
                _logger.LogInformation("Teacher {TeacherId} updated successfully", id);
                return result;
            }
            _logger.LogWarning("Cannot update teacher {TeacherId} - not found", id);
            throw new Exception("Id for this Teacher not found");
        }

        public async Task<Teacher> DeleteTeacherAsync(Guid id)
        {
            _logger.LogInformation("Deleting teacher {TeacherId}", id);
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if (existingTeacher != null)
            {
                var result = await _teacherRepository.DeleteTeacherAsync(id);
                _logger.LogInformation("Teacher {TeacherId} ({Name}) deleted successfully", id, existingTeacher.Name);
                return result;
            }
            _logger.LogWarning("Cannot delete teacher {TeacherId} - not found", id);
            throw new Exception("Id for this Teacher not found");
        }
    }
}
