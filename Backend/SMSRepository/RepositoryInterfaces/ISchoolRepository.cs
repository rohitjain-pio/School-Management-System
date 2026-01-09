using SMSDataModel.Model;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSDataModel.Model.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface ISchoolRepository
    {
        Task<bool> SchoolExistsAsync(School school);
        Task<IEnumerable<School>> GetAllSchoolsAsync();
        Task<List<SchoolDto>> GetAllSchoolsAsync(string schoolName);
        Task<School> GetSchoolByIdAsync(Guid schoolId);
        Task<School> CreateSchoolAsync(School school);
        Task<School> UpdateSchoolAsync(School updatedSchool);
        Task<School> DeleteSchoolAsync(School school);
    }
}
