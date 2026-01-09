using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.ServicesInterfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAnnouncemetsAsync(Guid schoolid);
        Task<Announcement> GetAnnouncementByIdAsync(Guid AnnoucementId);
        Task<Announcement> CreateAnnouncementAsync(CreateAnnouncementRqstDto createAnnouncement);
        Task<Announcement> UpdateAnnouncementAsync(Guid id, UpdateAnnouncementRequestDto updateAnnouncementRequestDto);
        Task<Announcement> DeleteAnnouncementAsync(Guid id);
    }
}
