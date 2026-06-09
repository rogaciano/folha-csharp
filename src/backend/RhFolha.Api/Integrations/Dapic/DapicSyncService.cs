using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using RhFolha.Domain.Integrations;
using RhFolha.Domain.Production;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Integrations.Dapic;

public sealed class DapicSyncService(RhFolhaDbContext dbContext, DapicClient dapicClient)
{
    private const int PageSize = 200;

    public async Task<DapicSyncResult> SyncEmployeesAsync(ExternalIntegration integration, Guid? userId, CancellationToken cancellationToken)
    {
        return await SyncPagedAsync(
            integration,
            "Employees",
            userId,
            (page, token) => dapicClient.GetEmployeesAsync(integration.BaseUrl, token, page, PageSize, cancellationToken),
            async items =>
            {
                var created = 0;
                var updated = 0;
                var externalIds = items.Select(item => item.Id.ToString()).ToList();
                var existing = await dbContext.DapicEmployees
                    .Where(employee => employee.CompanyId == integration.CompanyId && externalIds.Contains(employee.ExternalId))
                    .ToDictionaryAsync(employee => employee.ExternalId, cancellationToken);

                foreach (var item in items)
                {
                    var externalId = item.Id.ToString();
                    var name = FirstNotEmpty(item.Nome, item.NomeExibicao, item.Fantasia, externalId);

                    if (existing.TryGetValue(externalId, out var employee))
                    {
                        employee.UpdateFromSync(name, item.Fantasia, item.NomeExibicao);
                        updated += 1;
                    }
                    else
                    {
                        dbContext.DapicEmployees.Add(new DapicEmployee(integration.CompanyId, externalId, name, item.Fantasia, item.NomeExibicao));
                        created += 1;
                    }
                }

                return (created, updated);
            },
            cancellationToken);
    }

    public async Task<DapicSyncResult> SyncProductsAsync(ExternalIntegration integration, Guid? userId, CancellationToken cancellationToken)
    {
        return await SyncPagedAsync(
            integration,
            "Products",
            userId,
            (page, token) => dapicClient.GetProductsAsync(integration.BaseUrl, token, page, PageSize, cancellationToken),
            async items =>
            {
                var created = 0;
                var updated = 0;
                var externalIds = items.Select(item => item.Id.ToString()).ToList();
                var existing = await dbContext.ProductionProducts
                    .Where(product => product.CompanyId == integration.CompanyId && externalIds.Contains(product.ExternalId))
                    .ToDictionaryAsync(product => product.ExternalId, cancellationToken);

                foreach (var item in items)
                {
                    var externalId = item.Id.ToString();
                    var reference = FirstNotEmpty(ReadText(item.Referencia), ReadText(item.Nome), externalId);
                    var description = FirstNotEmpty(ReadText(item.DescricaoFabrica), ReadText(item.Descricao), reference);
                    var status = FirstNotEmpty(ReadText(item.Status), "Unknown");
                    var createdAt = ReadDateTime(item.DataCadastro);
                    var updatedAt = ReadDateTime(item.DataModificacao);

                    if (existing.TryGetValue(externalId, out var product))
                    {
                        product.UpdateFromSync(reference, description, status, createdAt, updatedAt);
                        updated += 1;
                    }
                    else
                    {
                        var newProduct = new ProductionProduct(integration.CompanyId, externalId, reference, description);
                        newProduct.UpdateFromSync(reference, description, status, createdAt, updatedAt);
                        dbContext.ProductionProducts.Add(newProduct);
                        created += 1;
                    }
                }

                return (created, updated);
            },
            cancellationToken);
    }

    public async Task<DapicSyncResult> SyncOperationsAsync(ExternalIntegration integration, Guid? userId, CancellationToken cancellationToken)
    {
        return await SyncNamedEntitiesAsync(
            integration,
            "ProductionOperations",
            userId,
            (page, token) => dapicClient.GetOperationsAsync(integration.BaseUrl, token, page, PageSize, cancellationToken),
            "operation",
            cancellationToken);
    }

    public async Task<DapicSyncResult> SyncCellsAsync(ExternalIntegration integration, Guid? userId, CancellationToken cancellationToken)
    {
        return await SyncNamedEntitiesAsync(
            integration,
            "ProductionCells",
            userId,
            (page, token) => dapicClient.GetCellsAsync(integration.BaseUrl, token, page, PageSize, cancellationToken),
            "cell",
            cancellationToken);
    }

    public async Task<DapicSyncResult> SyncProductionOrdersAsync(
        ExternalIntegration integration,
        Guid? userId,
        DateOnly startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        return await SyncPagedAsync(
            integration,
            "ProductionOrders",
            userId,
            (page, token) => dapicClient.GetProductionOrdersAsync(integration.BaseUrl, token, page, PageSize, startDate, endDate, cancellationToken),
            async items =>
            {
                var created = 0;
                var updated = 0;
                var externalIds = items.Select(item => item.Id.ToString()).ToList();
                var existing = await dbContext.ProductionOrders
                    .Where(order => order.CompanyId == integration.CompanyId && externalIds.Contains(order.ExternalId))
                    .ToDictionaryAsync(order => order.ExternalId, cancellationToken);

                foreach (var item in items)
                {
                    var externalId = item.Id.ToString();
                    var number = FirstNotEmpty(ReadText(item.Numero), ReadText(item.Codigo), ReadText(item.Lote), externalId);
                    var description = FirstNotEmpty(ReadText(item.Descricao), ReadText(item.Observacao), number);
                    var status = FirstNotEmpty(ReadText(item.Status), "Unknown");
                    var issueDate = ToDateOnly(ReadDateTime(item.DataConta) ?? ReadDateTime(item.DataCadastro));
                    var startDate = ToDateOnly(ReadDateTime(item.DataInicio));
                    var endDate = ToDateOnly(ReadDateTime(item.DataFim) ?? ReadDateTime(item.DataFinalizacao) ?? ReadDateTime(item.DataPrevisao));
                    var externalUpdatedAt = ReadDateTime(item.DataModificacao) ?? ReadDateTime(item.DataCadastro);

                    if (existing.TryGetValue(externalId, out var order))
                    {
                        order.UpdateFromSync(number, description, status, issueDate, startDate, endDate, externalUpdatedAt);
                        updated += 1;
                    }
                    else
                    {
                        var newOrder = new ProductionOrder(integration.CompanyId, externalId, number, description);
                        newOrder.UpdateFromSync(number, description, status, issueDate, startDate, endDate, externalUpdatedAt);
                        dbContext.ProductionOrders.Add(newOrder);
                        created += 1;
                    }
                }

                return (created, updated);
            },
            startDate,
            endDate,
            cancellationToken);
    }

    private async Task<DapicSyncResult> SyncNamedEntitiesAsync(
        ExternalIntegration integration,
        string resource,
        Guid? userId,
        Func<int, string, Task<DapicPagedResult<DapicNamedEntityDto>>> fetchPage,
        string entityType,
        CancellationToken cancellationToken)
    {
        return await SyncPagedAsync(
            integration,
            resource,
            userId,
            fetchPage,
            async items =>
            {
                var created = 0;
                var updated = 0;
                var externalIds = items.Select(item => item.Id.ToString()).ToList();

                if (entityType == "operation")
                {
                    var existing = await dbContext.ProductionOperations
                        .Where(operation => operation.CompanyId == integration.CompanyId && externalIds.Contains(operation.ExternalId))
                        .ToDictionaryAsync(operation => operation.ExternalId, cancellationToken);

                    foreach (var item in items)
                    {
                        var externalId = item.Id.ToString();
                        var name = FirstNotEmpty(item.Nome, item.Descricao, externalId);
                        var status = string.IsNullOrWhiteSpace(item.Status) ? "Unknown" : item.Status;

                        if (existing.TryGetValue(externalId, out var operation))
                        {
                            operation.UpdateFromSync(name, item.Descricao, status);
                            updated += 1;
                        }
                        else
                        {
                            var newOperation = new ProductionOperation(integration.CompanyId, externalId, name);
                            newOperation.UpdateFromSync(name, item.Descricao, status);
                            dbContext.ProductionOperations.Add(newOperation);
                            created += 1;
                        }
                    }
                }
                else
                {
                    var existing = await dbContext.ProductionCells
                        .Where(cell => cell.CompanyId == integration.CompanyId && externalIds.Contains(cell.ExternalId))
                        .ToDictionaryAsync(cell => cell.ExternalId, cancellationToken);

                    foreach (var item in items)
                    {
                        var externalId = item.Id.ToString();
                        var name = FirstNotEmpty(item.Nome, item.Descricao, externalId);
                        var status = string.IsNullOrWhiteSpace(item.Status) ? "Unknown" : item.Status;

                        if (existing.TryGetValue(externalId, out var cell))
                        {
                            cell.UpdateFromSync(name, item.Descricao, status);
                            updated += 1;
                        }
                        else
                        {
                            var newCell = new ProductionCell(integration.CompanyId, externalId, name);
                            newCell.UpdateFromSync(name, item.Descricao, status);
                            dbContext.ProductionCells.Add(newCell);
                            created += 1;
                        }
                    }
                }

                return (created, updated);
            },
            cancellationToken);
    }

    private async Task<DapicSyncResult> SyncPagedAsync<T>(
        ExternalIntegration integration,
        string resource,
        Guid? userId,
        Func<int, string, Task<DapicPagedResult<T>>> fetchPage,
        Func<IReadOnlyList<T>, Task<(int Created, int Updated)>> persistPage,
        CancellationToken cancellationToken)
    {
        return await SyncPagedAsync(integration, resource, userId, fetchPage, persistPage, null, null, cancellationToken);
    }

    private async Task<DapicSyncResult> SyncPagedAsync<T>(
        ExternalIntegration integration,
        string resource,
        Guid? userId,
        Func<int, string, Task<DapicPagedResult<T>>> fetchPage,
        Func<IReadOnlyList<T>, Task<(int Created, int Updated)>> persistPage,
        DateOnly? requestedFrom,
        DateOnly? requestedTo,
        CancellationToken cancellationToken)
    {
        var log = new ExternalSyncLog(integration.CompanyId, integration.Id, "Dapic", resource, userId);
        log.SetRequestedPeriod(requestedFrom, requestedTo);
        dbContext.ExternalSyncLogs.Add(log);

        var token = await GetValidTokenAsync(integration, cancellationToken);
        var page = 1;
        var totalPages = 1;
        var read = 0;
        var created = 0;
        var updated = 0;

        try
        {
            do
            {
                var result = await fetchPage(page, token);
                var items = result.Dados;
                read += items.Count;
                totalPages = result.TotalPaginas <= 0 ? 1 : result.TotalPaginas;

                var pageResult = await persistPage(items);
                created += pageResult.Created;
                updated += pageResult.Updated;
                page += 1;
            }
            while (page <= totalPages);

            integration.MarkSynced();
            log.Finish("Success", totalPages, read, created, updated, 0);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DapicSyncResult(resource, "Success", read, created, updated, 0, null);
        }
        catch (Exception exception)
        {
            integration.MarkError(exception.Message);
            log.Finish("Error", Math.Max(page - 1, 0), read, created, updated, 0, exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GetValidTokenAsync(ExternalIntegration integration, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(integration.AccessToken)
            && integration.AccessTokenExpiresAt.HasValue
            && integration.AccessTokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return integration.AccessToken;
        }

        var token = await dapicClient.LoginAsync(
            integration.BaseUrl,
            integration.ExternalCompanyIdentifier,
            integration.IntegrationTokenSecret,
            cancellationToken);

        integration.SetAccessToken(token.AccessToken, token.ExpiresAt);
        return token.AccessToken;
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }

    private static string? ReadText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static DateTime? ReadDateTime(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var cultures = new[] { new CultureInfo("pt-BR"), CultureInfo.InvariantCulture };
            foreach (var culture in cultures)
            {
                if (DateTime.TryParse(value, culture, DateTimeStyles.AssumeLocal, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        return null;
    }

    private static DateOnly? ToDateOnly(DateTime? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
    }
}

public sealed record DapicSyncResult(
    string Resource,
    string Status,
    int RecordsRead,
    int RecordsCreated,
    int RecordsUpdated,
    int RecordsIgnored,
    string? ErrorMessage);
