using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SMSPrototype1.Authorization
{
    public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceOwnerHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Task.CompletedTask;
            }

            // Get the resource user ID from route data
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var resourceUserId = httpContext.GetRouteValue("userId")?.ToString();
                
                if (string.IsNullOrEmpty(resourceUserId))
                {
                    resourceUserId = httpContext.GetRouteValue("id")?.ToString();
                }

                // If the user ID matches, they own the resource
                if (!string.IsNullOrEmpty(resourceUserId) && resourceUserId == userId)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
