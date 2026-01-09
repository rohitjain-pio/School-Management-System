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
    public class AttendanceRepository: IAttendanceRepository
    {
        private readonly DataContext _context;
        public AttendanceRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Attendance>> GetAllAttendancesOfStudentsAsync()
        {
            var result = await _context.Attendance.ToListAsync();
            return result;
        }
        public async Task<Attendance> GetAttendanceByIdAsync(Guid id)
        {
            var result = await _context.Attendance.FirstOrDefaultAsync(s => s.Id == id);
            return result;
        }
        public async Task<Attendance> CreateAttendanceAsync(Attendance newAttendanceRqst)
        {
            var result = await _context.Attendance.AddAsync(newAttendanceRqst);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<Attendance> updatedAttendanceAsync(Attendance updatedAttendance)
        {
            var result = _context.Attendance.Update(updatedAttendance);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<Attendance> DeleteAttendanceAsync(Attendance existingAttendance)
        {
            var result = _context.Attendance.Remove(existingAttendance);
            await _context.SaveChangesAsync();
            return existingAttendance;
        }
    }
}
