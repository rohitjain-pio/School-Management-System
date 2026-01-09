using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSDataModel.Model.ResponseDtos;
using SMSRepository.Repository;
using SMSRepository.RepositoryInterfaces;
using SMSServices.Services;
using SMSServices.ServicesInterfaces;
using System.Net;


namespace SMSPrototype1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService  schoolService;
        public SchoolController(ISchoolService schoolService)
        {
            this.schoolService = schoolService;
        }



        [HttpGet]
        [AllowAnonymous] // Public for registration
        public async Task<ApiResult<IEnumerable<School>>> GetAllSchoolsAsync()
        {
            var apiResult = new ApiResult<IEnumerable<School>>();
            try
            {
                apiResult.Content = await schoolService.GetAllSchoolsAsync();
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
        [HttpGet("search")]
        [AllowAnonymous] // Public for registration
        public async Task<ApiResult<IEnumerable<SchoolDto>>> GetAllSchoolsAsync([FromQuery] string schoolName)
        {
            var apiResult = new ApiResult<IEnumerable<SchoolDto>>();

            try
            {
                apiResult.Content = await schoolService.GetAllSchoolsAsync(schoolName);
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



        [HttpGet("getbyId/{schoolId}")]
        [Authorize]
        public async Task<ApiResult<School>> GetSchoolByIdAsync([FromRoute] Guid schoolId)
        {

            var apiResult = new ApiResult<School>();
            try
            {
                apiResult.Content = await schoolService.GetSchoolByIdAsync(schoolId);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "School with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        [HttpPost("CreateSchoolAsync")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ApiResult<School>> CreateSchoolAsync([FromBody] CreateSchoolRequestDto createSchoolRequest)
        {

            var apiResult = new ApiResult<School>();
            if (!ModelState.IsValid)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = string.Join(" | ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(e => e.ErrorMessage));
                return apiResult;
            }
            try
            {
                apiResult.Content = await schoolService.CreateSchoolAsync(createSchoolRequest);
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
        

        [HttpPut("UpdateSchool/{schoolId}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<School>> UpdateSchool([FromRoute] Guid schoolId, [FromBody] CreateSchoolRequestDto updateSchool)
        {

            var apiResult = new ApiResult<School>();
            try
            {
                apiResult.Content = await schoolService.UpdateSchoolAsync(schoolId, updateSchool);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "School with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

            
        }

        [HttpDelete("{schoolId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ApiResult<School>> DeleteSchool([FromRoute] Guid schoolId)
        {

            var apiResult = new ApiResult<School>();
            try
            {
                apiResult.Content = await schoolService.DeleteSchoolAsync(schoolId);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "School with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }


    }
}
