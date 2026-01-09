using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAttendancesOfStudentsAsync();
        Task<Attendance> GetAttendanceByIdAsync(Guid id);
        Task<Attendance> CreateAttendanceAsync(CreateAttendanceRqstDto newAttendanceRqst);
        Task<Attendance> updatedAttendanceAsync(Guid id, CreateAttendanceRqstDto updatedClass);
        Task<Attendance> DeleteAttendanceAsync(Guid id);
    }
}
