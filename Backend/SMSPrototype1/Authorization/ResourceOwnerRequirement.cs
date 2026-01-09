using Microsoft.AspNetCore.Authorization;

namespace SMSPrototype1.Authorization
{
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        public ResourceOwnerRequirement()
        {
        }
    }
}
