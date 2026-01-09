using SMSDataModel.Model.Models;
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
        Task<Student> GetStudentByIdAsync(Guid studentId);
        Task<IEnumerable<Student>> GetStudentByClassIdAsync(Guid classId);
        Task<Student> CreateStudentAsync(Student student);
        Task<Student> UpdateStudentAsync(Student updatedStudent);
        Task<Student> DeleteStudentAsync(Student student);
    }
}
