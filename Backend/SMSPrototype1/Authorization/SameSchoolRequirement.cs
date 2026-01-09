using Microsoft.AspNetCore.Authorization;

namespace SMSPrototype1.Authorization
{
    public class SameSchoolRequirement : IAuthorizationRequirement
    {
        public SameSchoolRequirement()
        {
        }
    }
}
