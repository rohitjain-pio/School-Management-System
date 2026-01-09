using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Detail {  get; set; }
        public DateOnly Date { get; set; }
        public string AnnouncedBy {  get; set; }
        public Guid SchoolId { get; set; }
        public School School { get; set; }
    }
}
