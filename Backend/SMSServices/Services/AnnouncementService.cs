using AutoMapper;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.Repository;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IMapper _mapper;
        public AnnouncementService(IAnnouncementRepository announcementRepository, IMapper mapper)
        {
            _announcementRepository = announcementRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<Announcement>> GetAllAnnouncemetsAsync(Guid schoolid)
        {
            return await _announcementRepository.GetAllAnnouncemetsAsync(schoolid);
        }
        public async Task<Announcement> GetAnnouncementByIdAsync(Guid AnnoucementId)
        {
            var result = await _announcementRepository.GetAnnouncementByIdAsync(AnnoucementId);
            if (result != null)
            {
                return result;
            }
            throw new Exception("Announcement not found!");
        }
        public async Task<Announcement> CreateAnnouncementAsync(CreateAnnouncementRqstDto createAnnouncement)
        {
            var newAnnouncement = _mapper.Map<Announcement>(createAnnouncement);
            var result = await _announcementRepository.CreateAnnouncementAsync(newAnnouncement);
            return result;
        }
        public async Task<Announcement> UpdateAnnouncementAsync(Guid id, UpdateAnnouncementRequestDto updateAnnouncementRequestDto)
        {
            var announcement = await _announcementRepository.GetAnnouncementByIdAsync(id);
            if (announcement != null)
            {
                announcement = _mapper.Map(updateAnnouncementRequestDto, announcement);
                var result = await _announcementRepository.UpdateAnnouncementAsync(announcement);
                return result;
            }
            throw new Exception("Announcement with this ID not found");
        }
        public async Task<Announcement> DeleteAnnouncementAsync(Guid id)
        {

            var existingAnnouncement = await _announcementRepository.GetAnnouncementByIdAsync(id);
            if (existingAnnouncement != null)
            {
                var result = await _announcementRepository.DeleteAnnouncementAsync(existingAnnouncement);
                return result;
            }
            throw new Exception("Attendance with this ID not found");
        }
    }
}
