using AutoMapper;
using SMSDataModel.Model.Models;
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
        public TeacherService(ITeacherRepository teacherRepository, IMapper mapper)
        {
            _teacherRepository = teacherRepository;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync(Guid schoolId)
        {
            return await _teacherRepository.GetAllTeachersAsync(schoolId);
        }
        public async Task<Teacher> GetTeacherByIdAsync(Guid id)
        {
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if (existingTeacher != null)
            {
                return existingTeacher;
            }
            throw new Exception("Id for this Teacher not found");
        }
        public async Task<Teacher> CreateTeacherAsync(CreateTeacherRqstDto teacherRqstDto)
        {
            var newTeacher = mapper.Map<Teacher>(teacherRqstDto);
            var result = await _teacherRepository.CreateTeacherAsync(newTeacher);
            return result;
        }

        public async Task<Teacher> UpdateTeacherAsync(Guid id, UpdateTeacherRequestDto updateTeacherRequestDto)
        {
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if(existingTeacher != null)
            {
                var teacher = mapper.Map(updateTeacherRequestDto, existingTeacher);
                var result = await _teacherRepository.UpdateTeacherAsync(teacher);
                return result;
            }
            throw new Exception("Id for this Teacher not found");
        }
        public async Task<Teacher> DeleteTeacherAsync(Guid id)
        {
            var existingTeacher = await _teacherRepository.GetTeacherByIdAsync(id);
            if (existingTeacher != null)
            {
                var result = await _teacherRepository.DeleteTeacherAsync(id);
                return result;
            }
            throw new Exception("Id for this Teacher not found");
        }
    }
}
        
