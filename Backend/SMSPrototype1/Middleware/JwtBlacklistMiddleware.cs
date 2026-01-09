using SMSServices.ServicesInterfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SMSPrototype1.Middleware
{
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklist)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (jti != null && await tokenBlacklist.IsBlacklistedAsync(jti))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Token revoked",
                        message = "This token has been revoked. Please log in again."
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
