using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SMSPrototype1.Authorization
{
    public class SameSchoolHandler : AuthorizationHandler<SameSchoolRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SameSchoolHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameSchoolRequirement requirement)
        {
            var userSchoolId = context.User.FindFirstValue("SchoolId");
            
            if (string.IsNullOrEmpty(userSchoolId))
            {
                return Task.CompletedTask;
            }

            // Get the resource school ID from route data or query string
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Try to get schoolId from route data
                var routeSchoolId = httpContext.GetRouteValue("schoolId")?.ToString();
                
                // Try to get from query string if not in route
                if (string.IsNullOrEmpty(routeSchoolId))
                {
                    routeSchoolId = httpContext.Request.Query["schoolId"].ToString();
                }

                // If we have a school ID to compare and they match, succeed
                if (!string.IsNullOrEmpty(routeSchoolId) && routeSchoolId == userSchoolId)
                {
                    context.Succeed(requirement);
                }
                else if (string.IsNullOrEmpty(routeSchoolId))
                {
                    // No school ID in request, allow for now (will be filtered by query)
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
