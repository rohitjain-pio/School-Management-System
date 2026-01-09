using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSDataModel.Model.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface ISchoolService
    {
        Task<IEnumerable<School>> GetAllSchoolsAsync();
        Task<List<SchoolDto>> GetAllSchoolsAsync(string schoolName);
        Task<School> GetSchoolByIdAsync(Guid schoolId);
        Task<School> CreateSchoolAsync(CreateSchoolRequestDto createSchool);
        Task<School> UpdateSchoolAsync(Guid id, CreateSchoolRequestDto updatedSchool);
        Task<School> DeleteSchoolAsync(Guid id);
    }
}
