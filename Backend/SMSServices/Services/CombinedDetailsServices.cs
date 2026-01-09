using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMSDataModel.Model.CombineModel;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;

namespace SMSServices.Services
{
    public class CombinedDetailsServices : ICombinedDetailsServices
    {

        private readonly ICombinedDetailsRepository _repository;

        public CombinedDetailsServices(ICombinedDetailsRepository repository)
        {
            _repository = repository;           
        }

        public async Task<HomeCombinedDetails> HomeCombinedDetail()
        {
            return await _repository.HomeCombinedDetail();
        }
        public async Task<HomeCombinedDetails> DashboardCombinedDetail(Guid schoolId)
        {
            return await _repository.DashboardCombinedDetail(schoolId);
        }



    }
}
