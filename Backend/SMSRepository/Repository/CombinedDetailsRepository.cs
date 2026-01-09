using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.CombineModel;
using SMSRepository.RepositoryInterfaces;

namespace SMSRepository.Repository
{
    public class CombinedDetailsRepository : ICombinedDetailsRepository
    {
        public readonly DataContext _Context;
        public CombinedDetailsRepository(DataContext Context)
        {
            _Context = Context;
        }


        public async Task<HomeCombinedDetails> HomeCombinedDetail()
        {
            var totalStudents = await _Context.Students.CountAsync();
            var totalSchools = await _Context.Schools.CountAsync();
            var totalClasses = await _Context.Classes.CountAsync();
            var totalTeachers = await _Context.Teachers.CountAsync();
            var totalPresentStudents = await _Context.Attendance.Where(x=>x.Date== DateOnly.FromDateTime(DateTime.Now) && x.Status.Equals("Present")).CountAsync();
            var totalPresentTeachers = await _Context.Attendance.Where(x=>x.Date== DateOnly.FromDateTime(DateTime.Now) && x.Status.Equals("Present")).CountAsync();
            var result = new HomeCombinedDetails
            {
                TotalSchools = totalSchools,
                TotalClasses = totalClasses,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                PresentStudents = totalPresentStudents,
                PresentTeachers = totalPresentTeachers
            };
            return result;
        }


        public async Task<HomeCombinedDetails> DashboardCombinedDetail(Guid schoolId)
        {
            // Need to correct
            var totalStudents = await _Context.Students.Where(x=>x.Id==schoolId).CountAsync();
            var totalSchools = await _Context.Schools.Where(x => x.Id == schoolId).CountAsync();
            var totalClasses = await _Context.Classes.Where(x => x.SchoolId == schoolId).CountAsync();
            var totalTeachers = await _Context.Teachers.Where(x => x.SchoolId == schoolId).CountAsync();
            //var totalPresentStudents = await _Context.Attendance.Where(x => x.SchoolId==schoolId && x.Date == DateOnly.FromDateTime(DateTime.Now) && x.Status.Equals("Present")).CountAsync();
            var totalPresentStudents = 0;
            //var totalPresentTeachers = await _Context.TeachersAttendance.Where(x => x.SchoolId == schoolId && x.Date == DateOnly.FromDateTime(DateTime.Now) && x.Status.Equals("Present")).CountAsync();
            var totalPresentTeachers = 0;
            var result = new HomeCombinedDetails
            {
                TotalSchools = totalSchools,
                TotalClasses = totalClasses,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                PresentStudents = totalPresentStudents,
                PresentTeachers = totalPresentTeachers
            };
            return result;
        }


    }
}
