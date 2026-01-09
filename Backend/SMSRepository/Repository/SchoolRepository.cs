using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SMSDataContext.Data;
using SMSDataModel.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SMSRepository.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.ResponseDtos;

namespace SMSRepository.Repository
{
    public class SchoolRepository : ISchoolRepository
    {
        public readonly DataContext _Context;
        public SchoolRepository(DataContext context)
        {
            _Context = context;
        }


        public async Task<bool> SchoolExistsAsync(School school)
        {
            var result = await _Context.Schools.FirstOrDefaultAsync(x => x.Name == school.Name && x.Email == school.Email && x.PinCode == school.PinCode);
            return result!=null ? true : false;
        }



        public async Task<IEnumerable<School>> GetAllSchoolsAsync()
        {
            return await _Context.Schools.ToListAsync();
        }
        public async Task<List<SchoolDto>> GetAllSchoolsAsync(string schoolName)
        {
            return await _Context.Schools
                .Where(s => EF.Functions.Like(s.Name, $"%{schoolName}%"))
                .Select(s => new SchoolDto{Id =  s.Id,Name = s.Name } )
                .ToListAsync();
        }


        public async Task<School> GetSchoolByIdAsync(Guid schoolId)
        {
            var existingSchool = await _Context.Schools.FindAsync(schoolId);
            return existingSchool;
        }

        public async Task<School> CreateSchoolAsync(School school)
        {
            await _Context.Schools.AddAsync(school);
            await _Context.SaveChangesAsync();
            return school;
        }

        public async Task<School> UpdateSchoolAsync(School updatedSchool)
        {
            var result =  _Context.Schools.Update(updatedSchool);
            await _Context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<School> DeleteSchoolAsync(School school)
        {
            var result = _Context.Schools.Remove(school);
            await _Context.SaveChangesAsync();
            return result.Entity;
        }

    }
}
