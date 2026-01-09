using SMSDataModel.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface ITeacherAttendanceRepository
    {
        Task<List<Attendance>> GetAllAttendancesOfTeachersAsync();
        Task<Attendance> GetTeacherByAttendanceIdAsync(Guid teacherAttendanceid);
        Task<Attendance> CreateTeacherAttendanceAsync(Attendance newTeacherAttendanceRqst);
        Task<Attendance> UpdateTeacherAttendandanceAsync(Attendance updatedTeacher);
        Task<Attendance> DeleteTeacherAttendanceAsync(Attendance existingTeacherAttendance);
    }
}
