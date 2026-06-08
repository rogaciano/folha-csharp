using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RhFolha.Domain.Security;

namespace RhFolha.Api.Security;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public AuthTokenResult CreateToken(SystemUser user)
    {
        var issuer = configuration["Authentication:Jwt:Issuer"] ?? "RhFolha";
        var audience = configuration["Authentication:Jwt:Audience"] ?? "RhFolhaFrontend";
        var secret = configuration["Authentication:Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException("Configure Authentication:Jwt:Secret com pelo menos 32 bytes.");
        }

        var expiresMinutes = configuration.GetValue("Authentication:Jwt:ExpiresMinutes", 480);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.CompanyId.HasValue)
        {
            claims.Add(new Claim("company_id", user.CompanyId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public sealed record AuthTokenResult(string Token, DateTime ExpiresAt);
