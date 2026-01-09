using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;

namespace SMSDataModel.Model.AutoMapper
{
    public class SchoolAutoMapper : Profile
    {

        public SchoolAutoMapper()
        {
            CreateMap<School, CreateSchoolRequestDto>().ReverseMap();
        }

    }
}
