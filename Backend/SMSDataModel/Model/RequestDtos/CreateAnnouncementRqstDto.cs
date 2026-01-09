using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateAnnouncementRqstDto
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public DateOnly Date { get; set; }
        public string AnnouncedBy { get; set; }
        public Guid SchoolId { get; set; }
    }
}