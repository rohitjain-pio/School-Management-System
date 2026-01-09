using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;
using System.Net;
using System.Security.Claims;

namespace SMSPrototype1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ITeacherService _teacherservice;
        public TeacherController(UserManager<ApplicationUser> userManager, ITeacherService teacherService)
        {
            this.userManager = userManager;
            _teacherservice = teacherService;
        }
        [HttpGet]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task <ApiResult<IEnumerable<Teacher>>> GetAllTeachersAsync()
        {
            var apiResult = new ApiResult<IEnumerable<Teacher>>();
            if (!ModelState.IsValid)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return apiResult;
            }
            try
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return SetError(apiResult, "Invalid or missing user ID.", HttpStatusCode.Unauthorized);
                }


                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return SetError(apiResult, "User not found.", HttpStatusCode.NotFound);
                }


                if (user.SchoolId == null)
                {
                    return SetError(apiResult, "User does not have a SchoolId assigned.", HttpStatusCode.BadRequest);
                }

                apiResult.Content = await _teacherservice.GetAllTeachersAsync(user.SchoolId);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch(Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }
        [HttpGet("{id}")]
        [Authorize(Policy = "TeacherOrAbove")]
        public async Task <ApiResult<Teacher>> GetTeacherByIdAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Teacher>();
            try
            {
                apiResult.Content = await _teacherservice.GetTeacherByIdAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Id for this Teacher not found"
               ? HttpStatusCode.NotFound
               : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }
        [HttpPost]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<Teacher>> CreateTeacherAsync([FromBody] CreateTeacherRqstDto teacherRqstDto)
        {
            var apiResult = new ApiResult<Teacher>();

            var schoolIdClaim = User.FindFirst("SchoolId");
            if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = HttpStatusCode.Unauthorized;
                apiResult.ErrorMessage = string.Join(" | ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(e => e.ErrorMessage));
                return apiResult;
            }

            
            teacherRqstDto.SchoolId = schoolId;

            try
            {
                apiResult.Content = await _teacherservice.CreateTeacherAsync(teacherRqstDto);
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

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<Teacher>> UpdateTeacherAsync([FromRoute] Guid id, [FromBody] UpdateTeacherRequestDto updateTeacherRequestDto)
        {
            var apiResult = new ApiResult<Teacher>();
            try
            {
                apiResult.Content = await _teacherservice.UpdateTeacherAsync(id, updateTeacherRequestDto);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Id for this Teacher not found"
                 ? HttpStatusCode.NotFound
                 : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<Teacher>> DeleteTeacherAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Teacher>();
            try
            {
                apiResult.Content = await _teacherservice.DeleteTeacherAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Id for this Teacher not found"
                ? HttpStatusCode.NotFound
                : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        private ApiResult<T> SetError<T>(ApiResult<T> result, string message, HttpStatusCode statusCode)
        {
            result.IsSuccess = false;
            result.StatusCode = statusCode;
            result.ErrorMessage = message;
            return result;
        }
    }

}
