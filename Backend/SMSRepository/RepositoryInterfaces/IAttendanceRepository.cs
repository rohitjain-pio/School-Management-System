using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface IAttendanceRepository
    {
        Task<List<Attendance>> GetAllAttendancesOfStudentsAsync();
        Task<Attendance> GetAttendanceByIdAsync(Guid id);
        Task<Attendance> CreateAttendanceAsync(Attendance newAttendanceRqst);
        Task<Attendance> updatedAttendanceAsync(Attendance updatedAttendance);
        Task<Attendance> DeleteAttendanceAsync(Attendance existingAttendance);
    }
}
