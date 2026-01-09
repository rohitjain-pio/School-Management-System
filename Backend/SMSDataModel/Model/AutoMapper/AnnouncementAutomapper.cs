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
    public class AnnouncementAutomapper : Profile
    {
        public AnnouncementAutomapper()
        {
            CreateMap<Announcement, CreateAnnouncementRqstDto>().ReverseMap();
            CreateMap<Announcement, UpdateAnnouncementRequestDto>().ReverseMap();
        }
    }
}
