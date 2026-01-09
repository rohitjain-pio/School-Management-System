using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.CombineModel;
using SMSRepository.RepositoryInterfaces;
using SMSServices.Services;
using SMSServices.ServicesInterfaces;

namespace SMSPrototype1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CombineController : ControllerBase
    {
        private readonly ICombinedDetailsServices _services;
        public CombineController(ICombinedDetailsServices services)
        {
            _services = services;            
        }

        [HttpGet]
        [AllowAnonymous] // Public stats for landing page
        public async Task<ApiResult<HomeCombinedDetails>> GetHomeCombinedDetail()
        {
            var apiResult = new ApiResult<HomeCombinedDetails>();

            try
            {
                apiResult.Content = await _services.HomeCombinedDetail();
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }


        [HttpGet("{schoolId}")]
        public async Task<ApiResult<HomeCombinedDetails>> GetDashboardCombinedDetail([FromRoute] Guid schoolId)
        {
            var apiResult = new ApiResult<HomeCombinedDetails>();

            try
            {
                apiResult.Content = await _services.DashboardCombinedDetail(schoolId);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }





    }
}
