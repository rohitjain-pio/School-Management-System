using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface ITeacherAttendanceService
    {
        Task<List<Attendance>> GetAllAttendancesOfTeachersAsync();
        Task<Attendance> GetTeacherByAttendanceIdAsync(Guid teacherAttendanceid);
        Task<Attendance> CreateTeacherAttendanceAsync(CreateTeacherAttendanceDto newTeacherAttendanceRqst);
        Task<Attendance> UpdatedTeacherAttendanceAsync(Guid id, CreateTeacherAttendanceDto updatedTeacher);
        Task<Attendance> DeleteTeacherAttendanceAsync(Guid id);
    }
}
