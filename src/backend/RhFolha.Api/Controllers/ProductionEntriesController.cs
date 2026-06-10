using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Production;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/production-entries")]
public sealed class ProductionEntriesController(
    RhFolhaDbContext dbContext,
    AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var entries = await dbContext.EmployeeProductionEntries
            .AsNoTracking()
            .Include(entry => entry.PayrollPeriod)
            .Where(entry => entry.DeletedAt == null)
            .OrderByDescending(entry => entry.ProductionDate)
            .ThenBy(entry => entry.EmployeeNameSnapshot)
            .Select(entry => new ProductionEntryResponse(
                entry.Id,
                entry.CompanyId,
                entry.PayrollPeriodId,
                entry.PayrollPeriod!.Code,
                entry.EmployeeId,
                entry.EmployeeRegistrationSnapshot,
                entry.EmployeeNameSnapshot,
                entry.ProductionDate,
                entry.ProductionOrderId,
                entry.OrderNumberSnapshot,
                entry.ProductionProductId,
                entry.ProductReferenceSnapshot,
                entry.ProductDescriptionSnapshot,
                entry.ProductionOperationId,
                entry.OperationNameSnapshot,
                entry.ProductionCellId,
                entry.CellNameSnapshot,
                entry.Quantity,
                entry.UnitValue,
                entry.TotalAmount,
                entry.RateSource,
                entry.ProductionRateId,
                entry.Origin,
                entry.Status,
                entry.Notes,
                entry.ApprovedAt))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }

    [HttpGet("catalogs")]
    public async Task<IActionResult> GetCatalogs(CancellationToken cancellationToken)
    {
        var products = await dbContext.ProductionProducts
            .AsNoTracking()
            .Where(product => product.DeletedAt == null)
            .OrderBy(product => product.Reference)
            .Select(product => new ProductionProductOption(
                product.Id,
                product.CompanyId,
                product.Reference,
                product.FactoryDescription,
                product.Status))
            .ToListAsync(cancellationToken);

        var operations = await dbContext.ProductionOperations
            .AsNoTracking()
            .Where(operation => operation.DeletedAt == null)
            .OrderBy(operation => operation.Name)
            .Select(operation => new ProductionNamedOption(
                operation.Id,
                operation.CompanyId,
                operation.Name,
                operation.Description,
                operation.Status))
            .ToListAsync(cancellationToken);

        var cells = await dbContext.ProductionCells
            .AsNoTracking()
            .Where(cell => cell.DeletedAt == null)
            .OrderBy(cell => cell.Name)
            .Select(cell => new ProductionNamedOption(
                cell.Id,
                cell.CompanyId,
                cell.Name,
                cell.Description,
                cell.Status))
            .ToListAsync(cancellationToken);

        var orders = await dbContext.ProductionOrders
            .AsNoTracking()
            .Where(order => order.DeletedAt == null)
            .OrderByDescending(order => order.IssueDate)
            .ThenBy(order => order.Number)
            .Take(200)
            .Select(order => new ProductionOrderOption(
                order.Id,
                order.CompanyId,
                order.Number,
                order.Description,
                order.Status,
                order.IssueDate))
            .ToListAsync(cancellationToken);

        return Ok(new ProductionCatalogResponse(products, operations, cells, orders));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateProductionEntryRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateHeader(request, cancellationToken);
        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var employee = validation.Employee!;
        var product = validation.Product!;
        var operation = validation.Operation!;
        var order = validation.Order;
        var cell = validation.Cell;
        var orderProduct = validation.OrderProduct;

        var resolvedRate = await ResolveProductionRate(
            request.CompanyId,
            request.ProductionDate,
            request.ProductionProductId,
            request.ProductionOperationId,
            request.ProductionCellId,
            employee.DepartmentId,
            employee.JobPositionId,
            request.Quantity,
            cancellationToken);

        if (resolvedRate.Result is not null)
        {
            return resolvedRate.Result;
        }

        try
        {
            var rate = resolvedRate.Rate!;
            var entry = new EmployeeProductionEntry(
                request.CompanyId,
                request.PayrollPeriodId,
                request.EmployeeId,
                request.ProductionDate,
                request.ProductionProductId,
                request.ProductionOperationId,
                request.Quantity,
                rate.UnitValue,
                employee.Registration,
                employee.Name,
                product.Reference,
                product.FactoryDescription,
                operation.Name);

            entry.ApplyRate(rate.Id, rate.UnitValue);
            entry.SetContext(
                request.ProductionOrderId,
                order?.Number,
                request.ProductionOrderProductId,
                request.ProductionCellId,
                cell?.Name,
                request.Notes);

            dbContext.EmployeeProductionEntries.Add(entry);
            auditService.Add("production_entry.create", "EmployeeProductionEntry", entry.Id, $"Apontamento de producao criado para {employee.Name}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = entry.Id }, new { entry.Id });
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, CreateProductionEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await dbContext.EmployeeProductionEntries
            .FirstOrDefaultAsync(item => item.Id == id && item.DeletedAt == null, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        if (entry.Status != "Draft")
        {
            return BadRequest(new { message = "Somente apontamentos em rascunho podem ser editados." });
        }

        var validation = await ValidateHeader(request, cancellationToken);
        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var employee = validation.Employee!;
        var product = validation.Product!;
        var operation = validation.Operation!;
        var order = validation.Order;
        var cell = validation.Cell;

        var resolvedRate = await ResolveProductionRate(
            request.CompanyId,
            request.ProductionDate,
            request.ProductionProductId,
            request.ProductionOperationId,
            request.ProductionCellId,
            employee.DepartmentId,
            employee.JobPositionId,
            request.Quantity,
            cancellationToken);

        if (resolvedRate.Result is not null)
        {
            return resolvedRate.Result;
        }

        try
        {
            var rate = resolvedRate.Rate!;
            entry.UpdateDraft(
                request.PayrollPeriodId,
                request.EmployeeId,
                request.ProductionDate,
                request.ProductionProductId,
                request.ProductionOperationId,
                request.Quantity,
                employee.Registration,
                employee.Name,
                product.Reference,
                product.FactoryDescription,
                operation.Name);
            entry.ApplyRate(rate.Id, rate.UnitValue);
            entry.SetContext(
                request.ProductionOrderId,
                order?.Number,
                request.ProductionOrderProductId,
                request.ProductionCellId,
                cell?.Name,
                request.Notes);

            auditService.Add("production_entry.update", "EmployeeProductionEntry", entry.Id, $"Apontamento de producao atualizado para {employee.Name}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var entry = await dbContext.EmployeeProductionEntries
            .FirstOrDefaultAsync(item => item.Id == id && item.DeletedAt == null, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        var period = await dbContext.PayrollPeriods.AsNoTracking().FirstAsync(period => period.Id == entry.PayrollPeriodId, cancellationToken);
        if (period.Status is not ("aberta" or "reaberta"))
        {
            return BadRequest(new { message = "A competencia precisa estar aberta ou reaberta para aprovar producao." });
        }

        try
        {
            entry.Approve(GetCurrentUserId());
            auditService.Add("production_entry.approve", "EmployeeProductionEntry", entry.Id, $"Apontamento de producao aprovado para {entry.EmployeeNameSnapshot}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var entry = await dbContext.EmployeeProductionEntries
            .FirstOrDefaultAsync(item => item.Id == id && item.DeletedAt == null, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        var period = await dbContext.PayrollPeriods.AsNoTracking().FirstAsync(period => period.Id == entry.PayrollPeriodId, cancellationToken);
        if (period.Status is not ("aberta" or "reaberta"))
        {
            return BadRequest(new { message = "A competencia precisa estar aberta ou reaberta para cancelar producao." });
        }

        try
        {
            entry.Cancel();
            auditService.Add("production_entry.cancel", "EmployeeProductionEntry", entry.Id, $"Apontamento de producao cancelado para {entry.EmployeeNameSnapshot}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private async Task<(IActionResult? Result, RhFolha.Domain.Payroll.PayrollPeriod? Period, RhFolha.Domain.Employees.Employee? Employee, ProductionProduct? Product, ProductionOperation? Operation, ProductionCell? Cell, ProductionOrder? Order, ProductionOrderProduct? OrderProduct)> ValidateHeader(
        CreateProductionEntryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return (BadRequest(new { message = "Quantidade deve ser maior que zero." }), null, null, null, null, null, null, null);
        }

        var period = await dbContext.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.PayrollPeriodId, cancellationToken);
        if (period is null)
        {
            return (BadRequest(new { message = "Competencia nao encontrada." }), null, null, null, null, null, null, null);
        }

        if (period.CompanyId != request.CompanyId)
        {
            return (BadRequest(new { message = "Competencia nao pertence a empresa informada." }), null, null, null, null, null, null, null);
        }

        if (period.Status is not ("aberta" or "reaberta"))
        {
            return (BadRequest(new { message = "Apontamentos so podem ser feitos em competencia aberta ou reaberta." }), null, null, null, null, null, null, null);
        }

        if (request.ProductionDate < period.StartsOn || request.ProductionDate > period.EndsOn)
        {
            return (BadRequest(new { message = "Data da producao deve estar dentro da competencia." }), null, null, null, null, null, null, null);
        }

        var employee = await dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.EmployeeId && item.CompanyId == request.CompanyId, cancellationToken);
        if (employee is null)
        {
            return (BadRequest(new { message = "Colaborador nao encontrado para a empresa informada." }), null, null, null, null, null, null, null);
        }

        var product = await dbContext.ProductionProducts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ProductionProductId && item.CompanyId == request.CompanyId && item.DeletedAt == null, cancellationToken);
        if (product is null)
        {
            return (BadRequest(new { message = "Produto de producao nao encontrado." }), null, null, null, null, null, null, null);
        }

        var operation = await dbContext.ProductionOperations.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ProductionOperationId && item.CompanyId == request.CompanyId && item.DeletedAt == null, cancellationToken);
        if (operation is null)
        {
            return (BadRequest(new { message = "Operacao de producao nao encontrada." }), null, null, null, null, null, null, null);
        }

        ProductionCell? cell = null;
        if (request.ProductionCellId.HasValue)
        {
            cell = await dbContext.ProductionCells.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ProductionCellId && item.CompanyId == request.CompanyId && item.DeletedAt == null, cancellationToken);
            if (cell is null)
            {
                return (BadRequest(new { message = "Celula de producao nao encontrada." }), null, null, null, null, null, null, null);
            }
        }

        ProductionOrder? order = null;
        if (request.ProductionOrderId.HasValue)
        {
            order = await dbContext.ProductionOrders.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ProductionOrderId && item.CompanyId == request.CompanyId && item.DeletedAt == null, cancellationToken);
            if (order is null)
            {
                return (BadRequest(new { message = "Ordem de producao nao encontrada." }), null, null, null, null, null, null, null);
            }
        }

        ProductionOrderProduct? orderProduct = null;
        if (request.ProductionOrderProductId.HasValue)
        {
            orderProduct = await dbContext.ProductionOrderProducts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ProductionOrderProductId && item.CompanyId == request.CompanyId && item.DeletedAt == null, cancellationToken);
            if (orderProduct is null)
            {
                return (BadRequest(new { message = "Produto da ordem de producao nao encontrado." }), null, null, null, null, null, null, null);
            }
        }

        return (null, period, employee, product, operation, cell, order, orderProduct);
    }

    private async Task<(IActionResult? Result, ProductionRate? Rate)> ResolveProductionRate(
        Guid companyId,
        DateOnly productionDate,
        Guid productId,
        Guid operationId,
        Guid? cellId,
        Guid departmentId,
        Guid jobPositionId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var rates = await dbContext.ProductionRates
            .AsNoTracking()
            .Include(rate => rate.ProductionRateTable)
            .Where(rate =>
                rate.CompanyId == companyId &&
                rate.Status == "Active" &&
                rate.DeletedAt == null &&
                rate.ProductionRateTable != null &&
                rate.ProductionRateTable.DeletedAt == null &&
                rate.ProductionRateTable.Status == "Active" &&
                rate.ProductionRateTable.EffectiveFrom <= productionDate &&
                (rate.ProductionRateTable.EffectiveTo == null || rate.ProductionRateTable.EffectiveTo >= productionDate) &&
                (rate.ProductionProductId == null || rate.ProductionProductId == productId) &&
                (rate.ProductionOperationId == null || rate.ProductionOperationId == operationId) &&
                (rate.ProductionCellId == null || rate.ProductionCellId == cellId) &&
                (rate.DepartmentId == null || rate.DepartmentId == departmentId) &&
                (rate.JobPositionId == null || rate.JobPositionId == jobPositionId) &&
                (rate.MinimumQuantity == null || rate.MinimumQuantity <= quantity) &&
                (rate.MaximumQuantity == null || rate.MaximumQuantity >= quantity))
            .ToListAsync(cancellationToken);

        if (rates.Count == 0)
        {
            return (BadRequest(new { message = "Nenhuma regra ativa de valor foi encontrada para este apontamento." }), null);
        }

        var ranked = rates
            .Select(rate => new RankedProductionRate(rate, Score(rate), TieBreaker(rate)))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.TieBreaker.HasProduct)
            .ThenByDescending(item => item.TieBreaker.HasOperation)
            .ThenByDescending(item => item.TieBreaker.HasCell)
            .ThenByDescending(item => item.TieBreaker.HasDepartment)
            .ThenByDescending(item => item.TieBreaker.HasJobPosition)
            .ThenByDescending(item => item.TieBreaker.MinimumQuantity)
            .ThenBy(item => item.TieBreaker.MaximumQuantity)
            .ToList();

        if (ranked.Count > 1 && ranked[0].HasSamePriorityAs(ranked[1]))
        {
            return (BadRequest(new { message = "Ha mais de uma regra de producao com a mesma prioridade. Ajuste a tabela antes de apontar." }), null);
        }

        return (null, ranked[0].Rate);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static int Score(ProductionRate rate)
    {
        return (rate.ProductionProductId.HasValue ? 32 : 0) +
            (rate.ProductionOperationId.HasValue ? 16 : 0) +
            (rate.ProductionCellId.HasValue ? 8 : 0) +
            (rate.DepartmentId.HasValue ? 4 : 0) +
            (rate.JobPositionId.HasValue ? 2 : 0) +
            (rate.MinimumQuantity.HasValue || rate.MaximumQuantity.HasValue ? 1 : 0);
    }

    private static ProductionRateTieBreaker TieBreaker(ProductionRate rate)
    {
        return new ProductionRateTieBreaker(
            rate.ProductionProductId.HasValue,
            rate.ProductionOperationId.HasValue,
            rate.ProductionCellId.HasValue,
            rate.DepartmentId.HasValue,
            rate.JobPositionId.HasValue,
            rate.MinimumQuantity ?? 0m,
            rate.MaximumQuantity ?? decimal.MaxValue);
    }
}

public sealed record CreateProductionEntryRequest(
    Guid CompanyId,
    Guid PayrollPeriodId,
    Guid EmployeeId,
    DateOnly ProductionDate,
    Guid? ProductionOrderId,
    Guid? ProductionOrderProductId,
    Guid ProductionProductId,
    Guid ProductionOperationId,
    Guid? ProductionCellId,
    decimal Quantity,
    string? Notes);

public sealed record ProductionEntryResponse(
    Guid Id,
    Guid CompanyId,
    Guid PayrollPeriodId,
    string PayrollPeriodCode,
    Guid EmployeeId,
    string EmployeeRegistration,
    string EmployeeName,
    DateOnly ProductionDate,
    Guid? ProductionOrderId,
    string? OrderNumber,
    Guid ProductionProductId,
    string ProductReference,
    string ProductDescription,
    Guid ProductionOperationId,
    string OperationName,
    Guid? ProductionCellId,
    string? CellName,
    decimal Quantity,
    decimal UnitValue,
    decimal TotalAmount,
    string RateSource,
    Guid? ProductionRateId,
    string Origin,
    string Status,
    string? Notes,
    DateTime? ApprovedAt);

public sealed record ProductionCatalogResponse(
    IReadOnlyList<ProductionProductOption> Products,
    IReadOnlyList<ProductionNamedOption> Operations,
    IReadOnlyList<ProductionNamedOption> Cells,
    IReadOnlyList<ProductionOrderOption> Orders);

public sealed record ProductionProductOption(
    Guid Id,
    Guid CompanyId,
    string Reference,
    string FactoryDescription,
    string Status);

public sealed record ProductionNamedOption(
    Guid Id,
    Guid CompanyId,
    string Name,
    string? Description,
    string Status);

public sealed record ProductionOrderOption(
    Guid Id,
    Guid CompanyId,
    string? Number,
    string? Description,
    string Status,
    DateOnly? IssueDate);

internal sealed record RankedProductionRate(ProductionRate Rate, int Score, ProductionRateTieBreaker TieBreaker)
{
    public bool HasSamePriorityAs(RankedProductionRate other)
    {
        return Score == other.Score && TieBreaker == other.TieBreaker;
    }
}

internal sealed record ProductionRateTieBreaker(
    bool HasProduct,
    bool HasOperation,
    bool HasCell,
    bool HasDepartment,
    bool HasJobPosition,
    decimal MinimumQuantity,
    decimal MaximumQuantity);
