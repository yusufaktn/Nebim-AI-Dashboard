using System.Text.Json;
using Entity.DTOs.AI;

namespace BLL.AI.Capabilities;

/// <summary>
/// Tüm capability'ler için base class.
/// Ortak işlevselliği sağlar.
/// </summary>
public abstract class BaseCapability : ICapability
{
    public abstract string Name { get; }
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract string Category { get; }
    public virtual string RequiredTier => "Free";
    public abstract List<CapabilityParameterDto> Parameters { get; }
    public abstract List<string> ExampleQueries { get; }

    public abstract Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default);

    public virtual ValidationResult ValidateParameters(JsonElement? parameters)
    {
        // Override in derived classes for specific validation
        return ValidationResult.Success();
    }

    public CapabilityInfoDto GetInfo()
    {
        return new CapabilityInfoDto
        {
            Name = Name,
            Versions = new List<string> { Version },
            ActiveVersion = Version,
            Description = Description,
            Category = Category,
            RequiredTier = RequiredTier,
            Parameters = Parameters,
            ExampleQueries = ExampleQueries,
            IsDeprecated = false
        };
    }

    /// <summary>
    /// Başarılı sonuç döndürür.
    /// </summary>
    protected CapabilityResultDto SuccessResult<T>(T data, long executionTimeMs, string dataSource = "simulation", int? recordCount = null)
    {
        return new CapabilityResultDto
        {
            CapabilityName = Name,
            CapabilityVersion = Version,
            IsSuccess = true,
            Data = JsonSerializer.SerializeToElement(data),
            ExecutionTimeMs = executionTimeMs,
            DataSource = dataSource,
            RecordCount = recordCount
        };
    }

    /// <summary>
    /// Hata sonucu döndürür.
    /// </summary>
    protected CapabilityResultDto ErrorResult(string errorMessage, string? errorCode = null, long executionTimeMs = 0)
    {
        return new CapabilityResultDto
        {
            CapabilityName = Name,
            CapabilityVersion = Version,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ExecutionTimeMs = executionTimeMs
        };
    }

    /// <summary>
    /// JSON'dan parametre okur.
    /// </summary>
    protected T? GetParameter<T>(JsonElement? parameters, string name, T? defaultValue = default)
    {
        if (parameters == null || parameters.Value.ValueKind == JsonValueKind.Null)
            return defaultValue;

        if (parameters.Value.TryGetProperty(name, out var property))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(property.GetRawText());
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Tarih parametresi okur, varsayılan değerlerle.
    /// </summary>
    protected (DateTime startDate, DateTime endDate) GetDateRange(JsonElement? parameters, int defaultDays = 30)
    {
        var endDate = GetParameter<DateTime?>(parameters, "endDate") ?? DateTime.UtcNow.Date;
        var startDate = GetParameter<DateTime?>(parameters, "startDate") ?? endDate.AddDays(-defaultDays);
        
        return (startDate, endDate);
    }
}
