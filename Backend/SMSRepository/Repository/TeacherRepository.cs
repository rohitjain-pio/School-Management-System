using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly DataContext _context;
        public TeacherRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync(Guid schoolId)
        {
            return await _context.Teachers.Where(x=>x.SchoolId==schoolId).ToListAsync();
        }
        public async Task<Teacher> GetTeacherByIdAsync(Guid id)
        {
            return await _context.Teachers.FirstOrDefaultAsync(s=> s.Id == id);
        }
        public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
        {
            await _context.Teachers.AddAsync(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }
        public async Task<Teacher> UpdateTeacherAsync(Teacher teacher)
        {
            await _context.SaveChangesAsync();
            return teacher;
        }
        public async Task<Teacher> DeleteTeacherAsync(Guid id)
        {
            var existingTeacher = await _context.Teachers.FindAsync(id);
             _context.Teachers.Remove(existingTeacher);
            _context.SaveChangesAsync();
            return existingTeacher;
        }

    }
}
