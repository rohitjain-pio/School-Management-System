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
    public class TeacherAttendanceService : ITeacherAttendanceService
    {
        private readonly ITeacherAttendanceRepository _teacherAttendanceRepository;
        private readonly IMapper mapper;
        public TeacherAttendanceService(ITeacherAttendanceRepository teacherAttendanceRepository, IMapper mapper)
        {
            _teacherAttendanceRepository = teacherAttendanceRepository;
            this.mapper = mapper;
        }
        public async Task<List<Attendance>> GetAllAttendancesOfTeachersAsync()
        {
            var result = await _teacherAttendanceRepository.GetAllAttendancesOfTeachersAsync();
            return result;
        }
        public async Task<Attendance> GetTeacherByAttendanceIdAsync(Guid teacherAttendanceid)
        {
            var result = await _teacherAttendanceRepository.GetTeacherByAttendanceIdAsync(teacherAttendanceid);
            if (result != null)
            {
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
        public async Task<Attendance> CreateTeacherAttendanceAsync(CreateTeacherAttendanceDto newTeacherAttendanceRqst)
        {
            var newteacherAttendence = mapper.Map<Attendance>(newTeacherAttendanceRqst);
            var result = await _teacherAttendanceRepository.CreateTeacherAttendanceAsync(newteacherAttendence);
            return result;
        }
        public async Task<Attendance> UpdatedTeacherAttendanceAsync(Guid id, CreateTeacherAttendanceDto updatedTeacher)
        {
            var TeacherAttendance = await _teacherAttendanceRepository.GetTeacherByAttendanceIdAsync(id);
            if (TeacherAttendance != null)
            {
                mapper.Map(updatedTeacher, TeacherAttendance);
                var result = await _teacherAttendanceRepository.UpdateTeacherAttendandanceAsync(TeacherAttendance);
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
        public async Task<Attendance> DeleteTeacherAttendanceAsync(Guid id)
        {

            var existingTeacherAttendance = await _teacherAttendanceRepository.GetTeacherByAttendanceIdAsync(id);
            if (existingTeacherAttendance != null)
            {
                var result = await _teacherAttendanceRepository.DeleteTeacherAttendanceAsync(existingTeacherAttendance);
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
    }
}
