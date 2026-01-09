using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMSServices.Services
{
    public interface IRoomAccessTokenService
    {
        string GenerateRoomAccessToken(Guid userId, Guid roomId, string username, string role = "Participant");
        ClaimsPrincipal? ValidateRoomAccessToken(string token);
        Guid? GetRoomIdFromToken(string token);
        Guid? GetUserIdFromToken(string token);
    }

    public class RoomAccessTokenService : IRoomAccessTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public RoomAccessTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? "SMSApp";
            _audience = _configuration["Jwt:Audience"] ?? "SMSApp";
        }

        public string GenerateRoomAccessToken(Guid userId, Guid roomId, string username, string role = "Participant")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("RoomId", roomId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), // Room access valid for 12 hours
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateRoomAccessToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Guid? GetRoomIdFromToken(string token)
        {
            var principal = ValidateRoomAccessToken(token);
            var roomIdClaim = principal?.FindFirst("RoomId")?.Value;
            
            if (Guid.TryParse(roomIdClaim, out var roomId))
                return roomId;
            
            return null;
        }

        public Guid? GetUserIdFromToken(string token)
        {
            var principal = ValidateRoomAccessToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            
            return null;
        }
    }
}
