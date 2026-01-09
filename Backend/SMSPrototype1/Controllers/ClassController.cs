using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.ServicesInterfaces;
using System.Net;
using System.Security.Claims;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly ISchoolClassServices schoolClassServices;
        private readonly UserManager<ApplicationUser> userManager;
        public ClassController(ISchoolClassServices schoolClassServices, UserManager<ApplicationUser> userManager)
        {
            this.schoolClassServices = schoolClassServices;
            this.userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<IEnumerable<SchoolClass>>> GetAllClassAsync()
        {
            var apiResult = new ApiResult<IEnumerable<SchoolClass>>();

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

                
                var classes = await schoolClassServices.GetAllClassesAsync(user.SchoolId);

                apiResult.Content = classes;
                apiResult.IsSuccess = true;
                apiResult.StatusCode = HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                return SetError(apiResult, ex.Message, HttpStatusCode.BadRequest);
            }
        }


        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<SchoolClass>> GetClassByIdAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<SchoolClass>();
            try
            {
                apiResult.Content = await schoolClassServices.GetClassByIdAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Class with this Id not found"
                    ? HttpStatusCode.NotFound
                    : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<SchoolClass>> CreateClassAsync([FromBody] CreateClassRequestDto newClass)
        {
            var apiResult = new ApiResult<SchoolClass>();

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
                // ✅ Set SchoolId from token if required
                var schoolIdClaim = User.FindFirst("SchoolId");
                if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
                {
                    return SetError(apiResult, "Missing or invalid SchoolId in token.", HttpStatusCode.Unauthorized);
                }

                newClass.SchoolId = schoolId; // Inject schoolId into request

                apiResult.Content = await schoolClassServices.CreateClassAsync(newClass);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<SchoolClass>> UpdateClassAsync([FromRoute] Guid id, [FromBody] UpdateClassRequestDto updatedClass)
        {
            var apiResult = new ApiResult<SchoolClass>();
            try
            {
                apiResult.Content = await schoolClassServices.UpdateClassAsync(id, updatedClass);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Class with this Id not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<SchoolClass>> DeleteClassAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<SchoolClass>();
            try
            {
                apiResult.Content = await schoolClassServices.DeleteClassAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Class with this Id not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }
        private Guid? GetSchoolIdFromClaims()
        {
            var schoolIdClaim = User.FindFirst("SchoolId")?.Value;

            if (Guid.TryParse(schoolIdClaim, out var schoolId))
            {
                return schoolId;
            }
            return null;
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
