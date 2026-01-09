using AutoMapper;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.AutoMapper
{
    public class TeacherAttendanceAutomapper : Profile
    {
        public TeacherAttendanceAutomapper()
        {
            //CreateMap<TeacherAttendance, CreateTeacherAttendanceDto>().ReverseMap();
        }
    }
}
