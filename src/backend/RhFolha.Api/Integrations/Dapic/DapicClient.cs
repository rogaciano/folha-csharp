using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RhFolha.Api.Integrations.Dapic;

public sealed class DapicClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DapicTokenResult> LoginAsync(
        string baseUrl,
        string empresa,
        string tokenIntegracao,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            Empresa = empresa,
            TokenIntegracao = tokenIntegracao
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(baseUrl, "/autenticacao/v1/login"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DapicClientException($"Falha ao autenticar no Dapic. HTTP {(int)response.StatusCode}: {content}");
        }

        using var document = JsonDocument.Parse(content);
        var accessToken = ReadString(document.RootElement, "access_token")
            ?? ReadString(document.RootElement, "accessToken")
            ?? ReadString(document.RootElement, "AccessToken")
            ?? ReadString(document.RootElement, "token")
            ?? ReadString(document.RootElement, "Token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new DapicClientException("Resposta de autenticacao do Dapic nao retornou access_token.");
        }

        var expiresIn = ReadInt(document.RootElement, "expires_in")
            ?? ReadInt(document.RootElement, "expiresIn")
            ?? ReadInt(document.RootElement, "ExpiresIn")
            ?? 86400;

        return new DapicTokenResult(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));
    }

    public Task<DapicPagedResult<DapicEmployeeDto>> GetEmployeesAsync(
        string baseUrl,
        string accessToken,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync<DapicEmployeeDto>(baseUrl, accessToken, $"/v1/funcionarios?Pagina={page}&RegistrosPorPagina={pageSize}", cancellationToken);
    }

    public Task<DapicPagedResult<DapicProductDto>> GetProductsAsync(
        string baseUrl,
        string accessToken,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync<DapicProductDto>(baseUrl, accessToken, $"/v1/produtos?Pagina={page}&RegistrosPorPagina={pageSize}", cancellationToken);
    }

    public Task<DapicPagedResult<DapicNamedEntityDto>> GetOperationsAsync(
        string baseUrl,
        string accessToken,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync<DapicNamedEntityDto>(baseUrl, accessToken, $"/v1/operacoesproducao?Pagina={page}&RegistrosPorPagina={pageSize}", cancellationToken);
    }

    public Task<DapicPagedResult<DapicNamedEntityDto>> GetCellsAsync(
        string baseUrl,
        string accessToken,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync<DapicNamedEntityDto>(baseUrl, accessToken, $"/v1/celulasproducao?Pagina={page}&RegistrosPorPagina={pageSize}", cancellationToken);
    }

    private async Task<DapicPagedResult<T>> GetPagedAsync<T>(
        string baseUrl,
        string accessToken,
        string path,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(baseUrl, path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DapicClientException($"Falha ao consultar Dapic. HTTP {(int)response.StatusCode}: {content}");
        }

        return JsonSerializer.Deserialize<DapicPagedResult<T>>(content, JsonOptions)
            ?? new DapicPagedResult<T>([], 0, 1, 0, 1);
    }

    private static Uri BuildUrl(string baseUrl, string path)
    {
        var normalizedBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "https://api.dapic.app"
            : baseUrl.Trim().TrimEnd('/');

        return new Uri($"{normalizedBaseUrl}{path}");
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : null;
    }
}

public sealed class DapicClientException(string message) : Exception(message);

public sealed record DapicTokenResult(string AccessToken, DateTime ExpiresAt);

public sealed record DapicPagedResult<T>(
    IReadOnlyList<T> Dados,
    int TotalRegistros,
    int Pagina,
    int RegistrosPorPagina,
    int TotalPaginas);

public sealed record DapicEmployeeDto(
    long Id,
    string? Nome,
    string? Fantasia,
    string? NomeExibicao);

public sealed record DapicProductDto(
    long Id,
    string? Referencia,
    string? DescricaoFabrica,
    DateTime? DataCadastro,
    DateTime? DataModificacao,
    string? Status);

public sealed record DapicNamedEntityDto(
    long Id,
    string? Nome,
    string? Descricao,
    string? Status);
