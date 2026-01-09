using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMSDataModel.Model.CombineModel;

namespace SMSRepository.RepositoryInterfaces
{
    public interface ICombinedDetailsRepository
    {
        Task<HomeCombinedDetails> HomeCombinedDetail();
        Task<HomeCombinedDetails> DashboardCombinedDetail(Guid schoolId);
    }
}
