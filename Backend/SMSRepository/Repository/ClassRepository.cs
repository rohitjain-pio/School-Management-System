using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model;
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
   public class ClassRepository : IClassRepository
    {
        private readonly DataContext _context;
        public ClassRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SchoolClass>> GetAllClassesAsync(Guid schoolId)
        {
            var result = await _context.Classes.Include("ClassTeacher").Where(x=>x.SchoolId==schoolId).ToListAsync();
            return result;
        }

        // Not now
        public async Task<SchoolClass> GetClassByIdAsync(Guid classId)
        {
            var result = await _context.Classes.FirstOrDefaultAsync(s => s.Id == classId);
            return result;
        }


        public async Task<SchoolClass> CreateClassAsync(SchoolClass newClass)
        {
            var result = await _context.Classes.AddAsync(newClass);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        
        public async Task<SchoolClass> UpdateClassAsync(SchoolClass updatedClass)
        {
            var result = _context.Classes.Update(updatedClass);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<SchoolClass> DeleteClassAsync(SchoolClass existingClass)
        {
            var result = _context.Classes.Remove(existingClass);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
    }
}

