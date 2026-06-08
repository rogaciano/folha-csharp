using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(
    RhFolhaDbContext dbContext,
    PasswordService passwordService,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = SystemUser.NormalizeEmail(request.Email);
        var user = await dbContext.SystemUsers.FirstOrDefaultAsync(
            item => item.Email == email,
            cancellationToken);

        if (user is null || !user.IsActive || !passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "E-mail ou senha invalidos." });
        }

        user.RegisterLogin();
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.CreateToken(user);

        return Ok(new LoginResponse(
            token.Token,
            token.ExpiresAt,
            new AuthUserResponse(user.Id, user.CompanyId, user.FullName, user.Email, user.Role)));
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt, AuthUserResponse User);

public sealed record AuthUserResponse(Guid Id, Guid? CompanyId, string FullName, string Email, string Role);
