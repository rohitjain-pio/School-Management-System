using SMSDataModel.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRepository.RepositoryInterfaces
{
    public interface IAnnouncementRepository
    {
        Task<IEnumerable<Announcement>> GetAllAnnouncemetsAsync(Guid schoolid);
        Task<Announcement> GetAnnouncementByIdAsync(Guid AnnoucementId);
        Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
        Task<Announcement> UpdateAnnouncementAsync(Announcement updatedAnnouncement);
        Task<Announcement> DeleteAnnouncementAsync(Announcement existingAnnouncement);
    }
}
