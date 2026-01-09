using AutoMapper;
using SMSDataModel.Model.Models;
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
    public class AttendanceService: IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper mapper;
        public AttendanceService(IAttendanceRepository attendanceRepository, IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            this.mapper = mapper;
        }
        public async Task<List<Attendance>> GetAllAttendancesOfStudentsAsync()
        {
            var result = await _attendanceRepository.GetAllAttendancesOfStudentsAsync();
            return result;
        }
        public async Task<Attendance> GetAttendanceByIdAsync(Guid id)
        {
            var result = await _attendanceRepository.GetAttendanceByIdAsync(id);
            if (result != null)
            {
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
        public async Task<Attendance> CreateAttendanceAsync(CreateAttendanceRqstDto newAttendanceRqst)
        {
            var newAttendence = mapper.Map<Attendance>(newAttendanceRqst);
            var result = await _attendanceRepository.CreateAttendanceAsync(newAttendence);
            return result;
        }
        public async Task<Attendance> updatedAttendanceAsync(Guid id, CreateAttendanceRqstDto updatedClass)
        {
            var Attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
            if (Attendance != null)
            {
                mapper.Map(updatedClass, Attendance);
                var result = await _attendanceRepository.updatedAttendanceAsync(Attendance);
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
        public async Task<Attendance> DeleteAttendanceAsync(Guid id)
        {

            var existingAttendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
            if (existingAttendance != null)
            {
                var result = await _attendanceRepository.DeleteAttendanceAsync(existingAttendance);
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
    }
}
