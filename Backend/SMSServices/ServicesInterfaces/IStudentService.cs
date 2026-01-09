using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentAsync(Guid schoolId);
        Task<Student> GetStudentByIdAsync(Guid studentId);
        Task<IEnumerable<Student>> GetStudentByClassIdAsync(Guid classId);
        Task<Student> CreateStudentAsync(CreateStudentRqstDto createStudent);
        Task<Student> UpdateStudentAsync(Guid id, UpdateStudentRequestDto updateStudentRequestDto);
        Task<Student> DeleteStudentAsync(Guid id);
    }
}
