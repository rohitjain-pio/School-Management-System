using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMSDataModel.Model.CombineModel;

namespace SMSServices.ServicesInterfaces
{
    public interface ICombinedDetailsServices
    {
        Task<HomeCombinedDetails> HomeCombinedDetail();
        Task<HomeCombinedDetails> DashboardCombinedDetail(Guid schoolId);
    }
}
