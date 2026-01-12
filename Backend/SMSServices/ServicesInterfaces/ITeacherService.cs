using SMSDataModel.Model.Models;
using SMSDataModel.Model.CombineModel;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<Teacher>> GetAllTeachersAsync(Guid schoolId);
        Task<PagedResult<Teacher>> GetAllTeachersPagedAsync(Guid schoolId, int pageNumber, int pageSize);
        Task<Teacher> GetTeacherByIdAsync(Guid id);
        Task<Teacher> CreateTeacherAsync(CreateTeacherRqstDto teacherRqstDto);
        Task<Teacher> UpdateTeacherAsync(Guid id, UpdateTeacherRequestDto  updateTeacherRequestDto);
        Task<Teacher> DeleteTeacherAsync(Guid id);
    }
}
