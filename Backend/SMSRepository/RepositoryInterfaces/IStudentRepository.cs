using SMSDataModel.Model.Models;
using SMSDataModel.Model.CombineModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllStudentAsync(Guid schoolId);
        Task<PagedResult<Student>> GetAllStudentPagedAsync(Guid schoolId, int pageNumber, int pageSize);
        Task<Student> GetStudentByIdAsync(Guid studentId);
        Task<IEnumerable<Student>> GetStudentByClassIdAsync(Guid classId);
        Task<PagedResult<Student>> GetStudentByClassIdPagedAsync(Guid classId, int pageNumber, int pageSize);
        Task<Student> CreateStudentAsync(Student student);
        Task<Student> UpdateStudentAsync(Student updatedStudent);
        Task<Student> DeleteStudentAsync(Student student);
    }
}
