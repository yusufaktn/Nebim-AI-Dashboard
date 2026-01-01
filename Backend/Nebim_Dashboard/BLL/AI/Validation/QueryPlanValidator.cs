using BLL.AI.Capabilities;
using Entity.DTOs.AI;
using Entity.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Validation;

/// <summary>
/// Query plan doğrulayıcı implementasyonu.
/// </summary>
public class QueryPlanValidator : IQueryPlanValidator
{
    private readonly ICapabilityRegistry _capabilityRegistry;
    private readonly ILogger<QueryPlanValidator> _logger;

    public QueryPlanValidator(
        ICapabilityRegistry capabilityRegistry,
        ILogger<QueryPlanValidator> logger)
    {
        _capabilityRegistry = capabilityRegistry;
        _logger = logger;
    }

    public Task<QueryValidationResult> ValidateAsync(
        QueryPlanDto plan, 
        int tenantId, 
        SubscriptionTier tier, 
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var restrictedCapabilities = new List<string>();

        // 1. Intent kontrolü
        if (plan.Intent == QueryIntent.OutOfScope)
        {
            errors.Add("Sorgu iş zekası kapsamında değil");
            return Task.FromResult(new QueryValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings
            });
        }

        // 2. Capability kontrolü
        if (!plan.Capabilities.Any())
        {
            errors.Add("Sorgu için uygun capability bulunamadı");
            return Task.FromResult(new QueryValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings
            });
        }

        // 3. Her capability'yi doğrula
        foreach (var capCall in plan.Capabilities)
        {
            var capability = _capabilityRegistry.GetCapability(capCall.Name, capCall.Version);

            if (capability == null)
            {
                errors.Add($"Capability bulunamadı: {capCall.Name}@{capCall.Version}");
                continue;
            }

            // Tier kontrolü
            var requiredTier = Enum.Parse<SubscriptionTier>(capability.RequiredTier);
            if (tier < requiredTier)
            {
                restrictedCapabilities.Add(capCall.Name);
                errors.Add($"'{capCall.Name}' capability'si {capability.RequiredTier} veya üzeri plan gerektiriyor");
                continue;
            }

            // Parametre validasyonu
            var paramValidation = capability.ValidateParameters(capCall.Parameters);
            if (!paramValidation.IsValid)
            {
                errors.AddRange(paramValidation.Errors.Select(e => $"{capCall.Name}: {e}"));
            }
            if (paramValidation.Warnings.Any())
            {
                warnings.AddRange(paramValidation.Warnings.Select(w => $"{capCall.Name}: {w}"));
            }
        }

        // 4. Dependency kontrolü
        var capabilityNames = plan.Capabilities.Select(c => c.Name).ToHashSet();
        foreach (var capCall in plan.Capabilities)
        {
            foreach (var dep in capCall.DependsOn)
            {
                if (!capabilityNames.Contains(dep))
                {
                    errors.Add($"'{capCall.Name}' capability'si '{dep}' capability'sine bağımlı ama '{dep}' planda yok");
                }
            }
        }

        // 5. Confidence uyarısı
        if (plan.Confidence < 0.7)
        {
            warnings.Add($"Sorgu güvenilirliği düşük: {plan.Confidence:P0}");
        }

        var result = new QueryValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings,
            RestrictedCapabilities = restrictedCapabilities
        };

        _logger.LogInformation(
            "Query plan validated for tenant {TenantId}. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
            tenantId, result.IsValid, errors.Count, warnings.Count);

        return Task.FromResult(result);
    }
}
