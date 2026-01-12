using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.CombineModel;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.Services;
using SMSServices.ServicesInterfaces;
using System.Net;
using System.Security.Claims;
//using System.Web.Http;

namespace SMSPrototype1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IStudentService _studentService;
        public StudentController(UserManager<ApplicationUser> userManager ,IStudentService studentService)
        {
            this.userManager = userManager;
            _studentService = studentService;
        }
        
        [HttpGet]
        [Authorize(Policy = "TeacherOrAbove")]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
        public async Task<ApiResult<PagedResult<Student>>> GetAllStudentAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var apiResult = new ApiResult<PagedResult<Student>>();
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
                
                // Validate pagination parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;
                
                apiResult.Content = await _studentService.GetAllStudentPagedAsync(user.SchoolId, pageNumber, pageSize);
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
        [HttpGet("{id}")]
        [Authorize(Policy = "TeacherOrAbove")]
        [ResponseCache(Duration = 120)]
        public async Task<ApiResult<Student>> GetStudentByIdAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Student>();
            try
            {
                apiResult.Content = await _studentService.GetStudentByIdAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Student with this ID not found"
               ? HttpStatusCode.NotFound
               : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }



        [HttpGet("GetStudentByClassIdAsync/{classId}")]
        [Authorize(Policy = "TeacherOrAbove")]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
        public async Task<ApiResult<PagedResult<Student>>> GetStudentByClassIdAsync([FromRoute] Guid classId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var apiResult = new ApiResult<PagedResult<Student>>();
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;
                
                apiResult.Content = await _studentService.GetStudentByClassIdPagedAsync(classId, pageNumber, pageSize);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Student with this ID not found"
               ? HttpStatusCode.NotFound
               : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }


        [HttpPost]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<Student>> CreateStudentAsync([FromBody] CreateStudentRqstDto createStudentRqstDto)
        {
            var apiResult = new ApiResult<Student>();

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
                apiResult.Content = await _studentService.CreateStudentAsync(createStudentRqstDto);
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
        public async Task<ApiResult<Student>> UpdateStudentAsync([FromRoute] Guid id, [FromBody] UpdateStudentRequestDto updateStudentRequestDto )
        {

            var apiResult = new ApiResult<Student>();
            try
            {
                apiResult.Content = await _studentService.UpdateStudentAsync(id, updateStudentRequestDto);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Student with this ID not found"
                ? HttpStatusCode.NotFound
                : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrSchoolAdmin")]
        public async Task<ApiResult<Student>> DeleteStudentAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Student>();
            try
            {
                apiResult.Content = await _studentService.DeleteStudentAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Id for this teacher not Found"
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
