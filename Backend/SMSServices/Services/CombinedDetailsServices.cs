using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SMSDataModel.Model.CombineModel;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;

namespace SMSServices.Services
{
    public class CombinedDetailsServices : ICombinedDetailsServices
    {

        private readonly ICombinedDetailsRepository _repository;
        private readonly IDistributedCache _cache;

        public CombinedDetailsServices(ICombinedDetailsRepository repository, IDistributedCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<HomeCombinedDetails> HomeCombinedDetail()
        {
            const string cacheKey = "dashboard_home_combined_details";
            
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<HomeCombinedDetails>(cachedData)!;
            }
            
            // Get from database
            var result = await _repository.HomeCombinedDetail();
            
            // Cache for 5 minutes
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
            
            return result;
        }
        
        public async Task<HomeCombinedDetails> DashboardCombinedDetail(Guid schoolId)
        {
            string cacheKey = $"dashboard_school_{schoolId}_combined_details";
            
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<HomeCombinedDetails>(cachedData)!;
            }
            
            // Get from database
            var result = await _repository.DashboardCombinedDetail(schoolId);
            
            // Cache for 2 minutes (school-specific data changes more frequently)
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
            
            return result;
        }



    }
}
