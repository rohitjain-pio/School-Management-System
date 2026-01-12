using SMSDataModel.Model.Models;
using SMSDataModel.Model.CombineModel;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface ITeacherRepository
    {
        Task<IEnumerable<Teacher>> GetAllTeachersAsync(Guid schoolId);
        Task<PagedResult<Teacher>> GetAllTeachersPagedAsync(Guid schoolId, int pageNumber, int pageSize);
        Task<Teacher> GetTeacherByIdAsync(Guid id);
        Task<Teacher> CreateTeacherAsync(Teacher teacher);
        Task<Teacher> UpdateTeacherAsync(Teacher teacher);
        Task<Teacher> DeleteTeacherAsync(Guid id);
    }
}
