using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Integrations.Dapic;
using RhFolha.Api.Security;
using RhFolha.Domain.Integrations;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleGroups.AdminOnly)]
[Route("api/integrations/dapic")]
public sealed class DapicIntegrationsController(
    RhFolhaDbContext dbContext,
    DapicClient dapicClient,
    DapicSyncService syncService,
    AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var integrations = await dbContext.ExternalIntegrations
            .AsNoTracking()
            .Where(integration => integration.Provider == "Dapic" && integration.DeletedAt == null)
            .OrderBy(integration => integration.Name)
            .Select(integration => new DapicIntegrationResponse(
                integration.Id,
                integration.CompanyId,
                integration.Name,
                integration.BaseUrl,
                integration.ExternalCompanyIdentifier,
                integration.AccessTokenExpiresAt,
                integration.LastSyncAt,
                integration.Status,
                integration.LastError))
            .ToListAsync(cancellationToken);

        return Ok(integrations);
    }

    [HttpPost("configure")]
    public async Task<IActionResult> Configure(ConfigureDapicIntegrationRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            return BadRequest(new { message = "Empresa informada nao encontrada." });
        }

        var integration = await dbContext.ExternalIntegrations
            .FirstOrDefaultAsync(item => item.CompanyId == request.CompanyId && item.Provider == "Dapic" && item.DeletedAt == null, cancellationToken);

        if (integration is null)
        {
            integration = new ExternalIntegration(
                request.CompanyId,
                "Dapic",
                request.Name,
                NormalizeBaseUrl(request.BaseUrl),
                request.ExternalCompanyIdentifier,
                request.IntegrationToken);
            dbContext.ExternalIntegrations.Add(integration);
            auditService.Add("dapic.configure", "ExternalIntegration", integration.Id, "Integracao Dapic configurada.");
        }
        else
        {
            integration.UpdateConfiguration(
                request.Name,
                NormalizeBaseUrl(request.BaseUrl),
                request.ExternalCompanyIdentifier,
                request.IntegrationToken);
            auditService.Add("dapic.update_configuration", "ExternalIntegration", integration.Id, "Configuracao da integracao Dapic atualizada.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new DapicIntegrationResponse(
            integration.Id,
            integration.CompanyId,
            integration.Name,
            integration.BaseUrl,
            integration.ExternalCompanyIdentifier,
            integration.AccessTokenExpiresAt,
            integration.LastSyncAt,
            integration.Status,
            integration.LastError));
    }

    [HttpPost("{id:guid}/test-connection")]
    public async Task<IActionResult> TestConnection(Guid id, CancellationToken cancellationToken)
    {
        var integration = await dbContext.ExternalIntegrations.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (integration is null)
        {
            return NotFound();
        }

        try
        {
            var token = await dapicClient.LoginAsync(
                integration.BaseUrl,
                integration.ExternalCompanyIdentifier,
                integration.IntegrationTokenSecret,
                cancellationToken);

            integration.SetAccessToken(token.AccessToken, token.ExpiresAt);
            auditService.Add("dapic.test_connection", "ExternalIntegration", integration.Id, "Conexao com Dapic testada com sucesso.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { status = "ok", tokenExpiresAt = token.ExpiresAt });
        }
        catch (Exception exception)
        {
            integration.MarkError(exception.Message);
            auditService.Add("dapic.test_connection_error", "ExternalIntegration", integration.Id, "Falha ao testar conexao com Dapic.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return BadRequest(new { status = "error", message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/sync/{resource}")]
    public async Task<IActionResult> Sync(
        Guid id,
        string resource,
        [FromQuery] DateOnly? dataInicial,
        [FromQuery] DateOnly? dataFinal,
        [FromBody] SyncDapicResourceRequest? request,
        CancellationToken cancellationToken)
    {
        var integration = await dbContext.ExternalIntegrations.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (integration is null)
        {
            return NotFound();
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = resource.ToLowerInvariant() switch
            {
                "employees" or "funcionarios" => await syncService.SyncEmployeesAsync(integration, userId, cancellationToken),
                "products" or "produtos" => await syncService.SyncProductsAsync(integration, userId, cancellationToken),
                "operations" or "operacoes" => await syncService.SyncOperationsAsync(integration, userId, cancellationToken),
                "cells" or "celulas" => await syncService.SyncCellsAsync(integration, userId, cancellationToken),
                "orders" or "ordens" or "ordensproducao" => await syncService.SyncProductionOrdersAsync(
                    integration,
                    userId,
                    request?.DataInicial ?? dataInicial ?? throw new InvalidOperationException("Informe a data inicial para sincronizar ordens de producao."),
                    request?.DataFinal ?? dataFinal,
                    cancellationToken),
                _ => throw new InvalidOperationException("Recurso de sincronizacao nao suportado.")
            };

            auditService.Add("dapic.sync", "ExternalIntegration", integration.Id, $"Sincronizacao Dapic executada: {result.Resource}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(new { status = "error", message = exception.Message });
        }
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<IActionResult> GetLogs(Guid id, CancellationToken cancellationToken)
    {
        var logs = await dbContext.ExternalSyncLogs
            .AsNoTracking()
            .Where(log => log.ExternalIntegrationId == id)
            .OrderByDescending(log => log.StartedAt)
            .Take(50)
            .Select(log => new DapicSyncLogResponse(
                log.Id,
                log.Resource,
                log.StartedAt,
                log.FinishedAt,
                log.Status,
                log.PageCount,
                log.RecordsRead,
                log.RecordsCreated,
                log.RecordsUpdated,
                log.RecordsIgnored,
                log.ErrorMessage))
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(CancellationToken cancellationToken)
    {
        var employees = await dbContext.DapicEmployees
            .AsNoTracking()
            .Include(employee => employee.Employee)
            .OrderBy(employee => employee.Name)
            .Select(employee => new DapicEmployeeResponse(
                employee.Id,
                employee.CompanyId,
                employee.ExternalId,
                employee.Name,
                employee.FantasyName,
                employee.DisplayName,
                employee.Status,
                employee.IsIgnored,
                employee.LinkStatus,
                employee.EmployeeId,
                employee.Employee != null ? employee.Employee.Registration : null,
                employee.Employee != null ? employee.Employee.Name : null,
                employee.LinkedAt,
                employee.IgnoredAt,
                employee.IgnoredReason,
                employee.LastSyncedAt))
            .ToListAsync(cancellationToken);

        return Ok(employees);
    }

    [HttpPost("employees/{id:guid}/link")]
    public async Task<IActionResult> LinkEmployee(Guid id, LinkDapicEmployeeRequest request, CancellationToken cancellationToken)
    {
        var dapicEmployee = await dbContext.DapicEmployees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

        if (dapicEmployee is null)
        {
            return NotFound();
        }

        var employee = await dbContext.Employees.FirstOrDefaultAsync(
            item => item.Id == request.EmployeeId && item.CompanyId == dapicEmployee.CompanyId,
            cancellationToken);

        if (employee is null)
        {
            return BadRequest(new { message = "Colaborador informado nao encontrado na mesma empresa." });
        }

        var alreadyLinked = await dbContext.DapicEmployees.AnyAsync(
            item => item.Id != dapicEmployee.Id && item.CompanyId == dapicEmployee.CompanyId && item.EmployeeId == employee.Id,
            cancellationToken);

        if (alreadyLinked)
        {
            return BadRequest(new { message = "Este colaborador ja esta vinculado a outro funcionario Dapic." });
        }

        dapicEmployee.LinkToEmployee(employee.Id);
        auditService.Add("dapic.employee_link", "DapicEmployee", dapicEmployee.Id, $"Funcionario Dapic {dapicEmployee.Name} vinculado ao colaborador {employee.Name}.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("employees/{id:guid}/ignore")]
    public async Task<IActionResult> IgnoreEmployee(Guid id, IgnoreDapicEmployeeRequest request, CancellationToken cancellationToken)
    {
        var dapicEmployee = await dbContext.DapicEmployees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

        if (dapicEmployee is null)
        {
            return NotFound();
        }

        dapicEmployee.Ignore(request.Reason);
        auditService.Add("dapic.employee_ignore", "DapicEmployee", dapicEmployee.Id, $"Funcionario Dapic {dapicEmployee.Name} marcado como ignorado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("employees/{id:guid}/reset-link")]
    public async Task<IActionResult> ResetEmployeeLink(Guid id, CancellationToken cancellationToken)
    {
        var dapicEmployee = await dbContext.DapicEmployees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

        if (dapicEmployee is null)
        {
            return NotFound();
        }

        dapicEmployee.ResetLink();
        auditService.Add("dapic.employee_reset_link", "DapicEmployee", dapicEmployee.Id, $"Vinculo do funcionario Dapic {dapicEmployee.Name} removido.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await dbContext.ProductionProducts
            .AsNoTracking()
            .OrderBy(product => product.Reference)
            .Select(product => new DapicProductResponse(
                product.Id,
                product.CompanyId,
                product.ExternalId,
                product.Reference,
                product.FactoryDescription,
                product.Status,
                product.LastSyncedAt))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("operations")]
    public async Task<IActionResult> GetOperations(CancellationToken cancellationToken)
    {
        var operations = await dbContext.ProductionOperations
            .AsNoTracking()
            .OrderBy(operation => operation.Name)
            .Select(operation => new DapicNamedProductionResponse(
                operation.Id,
                operation.CompanyId,
                operation.ExternalId,
                operation.Name,
                operation.Description,
                operation.Status,
                operation.LastSyncedAt))
            .ToListAsync(cancellationToken);

        return Ok(operations);
    }

    [HttpGet("cells")]
    public async Task<IActionResult> GetCells(CancellationToken cancellationToken)
    {
        var cells = await dbContext.ProductionCells
            .AsNoTracking()
            .OrderBy(cell => cell.Name)
            .Select(cell => new DapicNamedProductionResponse(
                cell.Id,
                cell.CompanyId,
                cell.ExternalId,
                cell.Name,
                cell.Description,
                cell.Status,
                cell.LastSyncedAt))
            .ToListAsync(cancellationToken);

        return Ok(cells);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await dbContext.ProductionOrders
            .AsNoTracking()
            .OrderByDescending(order => order.IssueDate)
            .ThenBy(order => order.Number)
            .Select(order => new DapicProductionOrderResponse(
                order.Id,
                order.CompanyId,
                order.ExternalId,
                order.Number,
                order.Description,
                order.Status,
                order.RawStatus,
                order.IssueDate,
                order.StartDate,
                order.EndDate,
                order.LastSyncedAt))
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        return string.IsNullOrWhiteSpace(baseUrl) ? "https://api.dapic.app" : baseUrl.Trim().TrimEnd('/');
    }
}

public sealed record ConfigureDapicIntegrationRequest(
    Guid CompanyId,
    string Name,
    string? BaseUrl,
    string ExternalCompanyIdentifier,
    string IntegrationToken);

public sealed record SyncDapicResourceRequest(DateOnly? DataInicial, DateOnly? DataFinal);
public sealed record LinkDapicEmployeeRequest(Guid EmployeeId);
public sealed record IgnoreDapicEmployeeRequest(string? Reason);

public sealed record DapicIntegrationResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string BaseUrl,
    string ExternalCompanyIdentifier,
    DateTime? AccessTokenExpiresAt,
    DateTime? LastSyncAt,
    string Status,
    string? LastError);

public sealed record DapicSyncLogResponse(
    Guid Id,
    string Resource,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string Status,
    int PageCount,
    int RecordsRead,
    int RecordsCreated,
    int RecordsUpdated,
    int RecordsIgnored,
    string? ErrorMessage);

public sealed record DapicEmployeeResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalId,
    string Name,
    string? FantasyName,
    string? DisplayName,
    string Status,
    bool IsIgnored,
    string LinkStatus,
    Guid? EmployeeId,
    string? EmployeeRegistration,
    string? EmployeeName,
    DateTime? LinkedAt,
    DateTime? IgnoredAt,
    string? IgnoredReason,
    DateTime LastSyncedAt);

public sealed record DapicProductResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalId,
    string Reference,
    string FactoryDescription,
    string Status,
    DateTime LastSyncedAt);

public sealed record DapicNamedProductionResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalId,
    string Name,
    string? Description,
    string Status,
    DateTime LastSyncedAt);

public sealed record DapicProductionOrderResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalId,
    string? Number,
    string? Description,
    string Status,
    string? RawStatus,
    DateOnly? IssueDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateTime LastSyncedAt);
