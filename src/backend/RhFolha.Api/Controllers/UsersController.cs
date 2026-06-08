using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleGroups.AdminOnly)]
[Route("api/users")]
public sealed class UsersController(
    RhFolhaDbContext dbContext,
    PasswordService passwordService,
    AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var users = await dbContext.SystemUsers
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new UserResponse(
                user.Id,
                user.CompanyId,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive,
                user.LastLoginAt))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.CompanyId.HasValue && !await CompanyExists(request.CompanyId.Value, cancellationToken))
        {
            return BadRequest(new { message = "Empresa informada nao encontrada." });
        }

        var email = SystemUser.NormalizeEmail(request.Email);
        var exists = await dbContext.SystemUsers.AnyAsync(user => user.Email == email, cancellationToken);

        if (exists)
        {
            return Conflict(new { message = "Ja existe um usuario com este e-mail." });
        }

        try
        {
            var user = new SystemUser(
                request.CompanyId,
                request.FullName,
                email,
                passwordService.Hash(request.Password),
                request.Role);

            dbContext.SystemUsers.Add(user);
            auditService.Add("user.create", "SystemUser", user.Id, $"Usuario {user.Email} criado com perfil {user.Role}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, null);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/update")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.SystemUsers.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (request.CompanyId.HasValue && !await CompanyExists(request.CompanyId.Value, cancellationToken))
        {
            return BadRequest(new { message = "Empresa informada nao encontrada." });
        }

        try
        {
            var previousRole = user.Role;
            user.UpdateProfile(request.CompanyId, request.FullName, request.Role);

            if (previousRole == SystemRoles.Administrator && user.Role != SystemRoles.Administrator)
            {
                var activeAdmins = await CountActiveAdminsExcept(user.Id, cancellationToken);
                if (user.IsActive)
                {
                    activeAdmins += 1;
                }

                if (activeAdmins == 0)
                {
                    return BadRequest(new { message = "O sistema precisa manter pelo menos um administrador ativo." });
                }
            }

            auditService.Add("user.update", "SystemUser", user.Id, $"Usuario {user.Email} atualizado para perfil {user.Role}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.SystemUsers.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        try
        {
            user.ResetPassword(passwordService.Hash(request.Password));
            auditService.Add("user.reset_password", "SystemUser", user.Id, $"Senha do usuario {user.Email} redefinida.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.SystemUsers.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        user.Activate();
        auditService.Add("user.activate", "SystemUser", user.Id, $"Usuario {user.Email} reativado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == id)
        {
            return BadRequest(new { message = "Usuario nao pode inativar a propria conta." });
        }

        var user = await dbContext.SystemUsers.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (user.Role == SystemRoles.Administrator && await CountActiveAdminsExcept(user.Id, cancellationToken) == 0)
        {
            return BadRequest(new { message = "O sistema precisa manter pelo menos um administrador ativo." });
        }

        user.Deactivate();
        auditService.Add("user.deactivate", "SystemUser", user.Id, $"Usuario {user.Email} inativado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<bool> CompanyExists(Guid companyId, CancellationToken cancellationToken)
    {
        return await dbContext.Companies.AnyAsync(company => company.Id == companyId, cancellationToken);
    }

    private async Task<int> CountActiveAdminsExcept(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.SystemUsers.CountAsync(
            user => user.Id != userId && user.IsActive && user.Role == SystemRoles.Administrator,
            cancellationToken);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}

public sealed record CreateUserRequest(Guid? CompanyId, string FullName, string Email, string Password, string Role);

public sealed record UpdateUserRequest(Guid? CompanyId, string FullName, string Role);

public sealed record ResetPasswordRequest(string Password);

public sealed record UserResponse(
    Guid Id,
    Guid? CompanyId,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt);
