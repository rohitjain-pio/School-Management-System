using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.ApiResult;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.Services;
using SMSServices.ServicesInterfaces;
using System.Net;

namespace SMSPrototype1.Controllers
{
    [Authorize(Policy = "TeacherOrAbove")]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        public async Task<ApiResult<IEnumerable<Attendance>>> GetAllAttendancesOfStudentsAsync()
        {

            var apiResult = new ApiResult<IEnumerable<Attendance>>();
            try
            {
                apiResult.Content = await _attendanceService.GetAllAttendancesOfStudentsAsync();
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
        public async Task<ApiResult<Attendance>> GetAttendanceByIdAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Attendance>();
            try
            {
                apiResult.Content = await _attendanceService.GetAttendanceByIdAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Attendance with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }
        [HttpPost]
        public async Task<ApiResult<Attendance>> CreateAttendanceAsync([FromBody] CreateAttendanceRqstDto newAttendance)
        {
            var apiResult = new ApiResult<Attendance>();
            try
            {
                apiResult.Content = await _attendanceService.CreateAttendanceAsync(newAttendance);
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
        public async Task<ApiResult<Attendance>> UpdateAttendandanceAsync([FromRoute] Guid id, [FromBody] CreateAttendanceRqstDto updatedAttendance)
        {
            var apiResult = new ApiResult<Attendance>();
            try
            {
                apiResult.Content = await _attendanceService.updatedAttendanceAsync(id, updatedAttendance);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Attendance with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }
        }
        [HttpDelete("{id}")]
        public async Task<ApiResult<Attendance>> DeleteAttendanceAsync([FromRoute] Guid id)
        {
            var apiResult = new ApiResult<Attendance>();
            try
            {
                apiResult.Content = await _attendanceService.DeleteAttendanceAsync(id);
                apiResult.IsSuccess = true;
                apiResult.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.IsSuccess = false;
                apiResult.StatusCode = ex.Message == "Attendance with this ID not found"
                   ? HttpStatusCode.NotFound
                   : HttpStatusCode.BadRequest;
                apiResult.ErrorMessage = ex.Message;
                return apiResult;
            }

        }

    }
}
