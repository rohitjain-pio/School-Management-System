using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;

namespace SMSServices.ServicesInterfaces
{
    public interface ISchoolClassServices
    {
        Task<List<SchoolClass>> GetAllClassesAsync(Guid schoolId);
        Task<SchoolClass> GetClassByIdAsync(Guid id);
        Task<SchoolClass> CreateClassAsync(CreateClassRequestDto createClassRequestDto);
        Task<SchoolClass> UpdateClassAsync(Guid id, UpdateClassRequestDto updatedClass);
        Task<SchoolClass> DeleteClassAsync(Guid id);
    }
}
