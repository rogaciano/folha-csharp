using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace RhFolha.Api.Integrations.Dapic;

public sealed class DapicClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    static DapicClient()
    {
        JsonOptions.Converters.Add(new FlexibleIntJsonConverter());
    }

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

    public Task<DapicPagedResult<DapicProductionOrderDto>> GetProductionOrdersAsync(
        string baseUrl,
        string accessToken,
        int page,
        int pageSize,
        DateOnly startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        var path = $"/v1/ordensproducao?Pagina={page}&RegistrosPorPagina={pageSize}&DataInicial={startDate:yyyy-MM-dd}";
        if (endDate.HasValue)
        {
            path += $"&DataFinal={endDate.Value:yyyy-MM-dd}";
        }

        return GetPagedAsync<DapicProductionOrderDto>(baseUrl, accessToken, path, cancellationToken);
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

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var items = DeserializeItems<T>(root);

        return new DapicPagedResult<T>(
            items,
            ReadInt(root, "TotalRegistros") ?? items.Count,
            ReadInt(root, "Pagina") ?? 1,
            ReadInt(root, "RegistrosPorPagina") ?? items.Count,
            ReadInt(root, "TotalPaginas") ?? 1);
    }

    private static IReadOnlyList<T> DeserializeItems<T>(JsonElement root)
    {
        var itemsElement = TryGetProperty(root, "Dados")
            ?? TryGetProperty(root, "dados")
            ?? TryGetProperty(root, "Data")
            ?? TryGetProperty(root, "data");

        if (itemsElement is null || itemsElement.Value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return JsonSerializer.Deserialize<IReadOnlyList<T>>(itemsElement.Value.GetRawText(), JsonOptions) ?? [];
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
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.ToString(),
            _ => null
        };
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        var property = TryGetProperty(element, propertyName);
        if (property is null)
        {
            return null;
        }

        if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var numberValue))
        {
            return numberValue;
        }

        if (property.Value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = property.Value.GetString();
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue)
            ? decimal.ToInt32(decimal.Truncate(decimalValue))
            : null;
    }

    private static JsonElement? TryGetProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var exactProperty))
        {
            return exactProperty;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }
}

public sealed class DapicClientException(string message) : Exception(message);

internal sealed class FlexibleIntJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numberValue))
        {
            return numberValue;
        }

        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out var stringValue))
        {
            return stringValue;
        }

        throw new JsonException("Valor inteiro invalido retornado pela Dapic.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

public sealed record DapicTokenResult(string AccessToken, DateTime ExpiresAt);

public sealed record DapicPagedResult<T>(
    IReadOnlyList<T> Dados,
    int TotalRegistros,
    int Pagina,
    int RegistrosPorPagina,
    int TotalPaginas);

public sealed record DapicEmployeeDto(
    JsonElement Id,
    string? Nome,
    string? Fantasia,
    string? NomeExibicao);

public sealed record DapicProductDto(
    JsonElement Id,
    string? Referencia,
    string? DescricaoFabrica,
    DateTime? DataCadastro,
    DateTime? DataModificacao,
    string? Status);

public sealed record DapicNamedEntityDto(
    JsonElement Id,
    string? Nome,
    string? Descricao,
    string? Status);

public sealed record DapicProductionOrderDto(
    JsonElement Id,
    string? Numero,
    string? Codigo,
    string? Lote,
    string? Descricao,
    string? Observacao,
    string? Status,
    DateTime? DataConta,
    DateTime? DataCadastro,
    DateTime? DataInicio,
    DateTime? DataPrevisao,
    DateTime? DataFim,
    DateTime? DataFinalizacao,
    DateTime? DataModificacao);
