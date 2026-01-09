using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSRepository.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.Repository
{
    public class TeacherAttendanceRepository : ITeacherAttendanceRepository
    {
        private readonly DataContext _context;
        public TeacherAttendanceRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Attendance>> GetAllAttendancesOfTeachersAsync()
        {
            var result = await _context.Attendance.ToListAsync();
            return result;
        }
        public async Task<Attendance> GetTeacherByAttendanceIdAsync(Guid teacherAttendanceid)
        {
            var result = await _context.Attendance.FirstOrDefaultAsync(s => s.Id == teacherAttendanceid);
            return result;
        }
        public async Task<Attendance> CreateTeacherAttendanceAsync(Attendance newTeacherAttendanceRqst)
        {
            var result = await _context.Attendance.AddAsync(newTeacherAttendanceRqst);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<Attendance> UpdateTeacherAttendandanceAsync(Attendance updatedTeacher)
        {
            var result = _context.Attendance.Update(updatedTeacher);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<Attendance> DeleteTeacherAttendanceAsync(Attendance existingTeacherAttendance)
        {
            var result = _context.Attendance.Remove(existingTeacherAttendance);
            await _context.SaveChangesAsync();
            return existingTeacherAttendance;
        }
    }
}
