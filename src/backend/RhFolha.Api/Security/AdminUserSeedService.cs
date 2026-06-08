using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Security;

public sealed class AdminUserSeedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<AdminUserSeedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RhFolhaDbContext>();

        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            return;
        }

        if (await dbContext.SystemUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();
        var companyId = await dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.CreatedAt)
            .Select(company => (Guid?)company.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var email = configuration["Authentication:SeedAdmin:Email"] ?? "admin@rhfolha.local";
        var password = configuration["Authentication:SeedAdmin:Password"] ?? "Admin@123";
        var fullName = configuration["Authentication:SeedAdmin:FullName"] ?? "Administrador";
        var user = new SystemUser(companyId, fullName, email, passwordService.Hash(password), "administrador");

        dbContext.SystemUsers.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Usuario administrador inicial criado: {Email}. Troque a senha/configuracao antes de producao.", email);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
