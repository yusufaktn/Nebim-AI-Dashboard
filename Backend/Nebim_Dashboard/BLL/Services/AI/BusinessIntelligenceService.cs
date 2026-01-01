using System.Diagnostics;
using System.Text.Json;
using BLL.AI.Orchestrator;
using BLL.AI.Planner;
using BLL.AI.Validation;
using DAL.Context;
using Entity.App;
using Entity.DTOs.AI;
using Entity.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services.AI;

/// <summary>
/// İş zekası servisi implementasyonu.
/// Tüm AI pipeline'ını yönetir: Planlama → Doğrulama → Çalıştırma → Kayıt
/// </summary>
public class BusinessIntelligenceService : IBusinessIntelligenceService
{
    private readonly IQueryPlanner _queryPlanner;
    private readonly IQueryPlanValidator _queryPlanValidator;
    private readonly ISubscriptionValidator _subscriptionValidator;
    private readonly ITenantValidator _tenantValidator;
    private readonly IQueryOrchestrator _queryOrchestrator;
    private readonly BLL.AI.Capabilities.ICapabilityRegistry _capabilityRegistry;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BusinessIntelligenceService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BusinessIntelligenceService(
        IQueryPlanner queryPlanner,
        IQueryPlanValidator queryPlanValidator,
        ISubscriptionValidator subscriptionValidator,
        ITenantValidator tenantValidator,
        IQueryOrchestrator queryOrchestrator,
        BLL.AI.Capabilities.ICapabilityRegistry capabilityRegistry,
        AppDbContext dbContext,
        ILogger<BusinessIntelligenceService> logger)
    {
        _queryPlanner = queryPlanner;
        _queryPlanValidator = queryPlanValidator;
        _subscriptionValidator = subscriptionValidator;
        _tenantValidator = tenantValidator;
        _queryOrchestrator = queryOrchestrator;
        _capabilityRegistry = capabilityRegistry;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BusinessQueryResponse> ProcessQueryAsync(
        BusinessQueryRequest request,
        int tenantId,
        int userId,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new BusinessQueryResponse
        {
            OriginalQuery = request.Query,
            Timestamp = DateTime.UtcNow
        };

        QueryPlanDto? queryPlan = null;
        int tokensUsed = 0;

        try
        {
            // 1. Tenant doğrulama
            var tenantValidation = await _tenantValidator.ValidateAsync(tenantId, ct);
            if (!tenantValidation.IsValid)
            {
                response.IsSuccess = false;
                response.ErrorCode = "TENANT_VALIDATION_FAILED";
                response.ErrorMessage = tenantValidation.Error;
                await SaveQueryHistoryAsync(tenantId, userId, request.Query, null, response, 0, stopwatch.ElapsedMilliseconds);
                return response;
            }

            // 2. Subscription kota kontrolü
            var quotaCheck = await _subscriptionValidator.CheckQuotaAsync(tenantId, ct);
            if (!quotaCheck.HasQuota)
            {
                response.IsSuccess = false;
                response.ErrorCode = "QUOTA_EXCEEDED";
                response.ErrorMessage = quotaCheck.Message;
                await SaveQueryHistoryAsync(tenantId, userId, request.Query, null, response, 0, stopwatch.ElapsedMilliseconds);
                return response;
            }

            // 3. AI ile sorgu planlama
            var planResult = await _queryPlanner.CreatePlanAsync(
                request.Query,
                tenantId,
                tenantValidation.Tier,
                null, // ConversationHistory not used in this version
                ct);

            tokensUsed = planResult.TokensUsed;

            if (!planResult.Success || planResult.Plan == null)
            {
                response.IsSuccess = false;
                response.ErrorCode = "PLANNING_FAILED";
                response.ErrorMessage = planResult.Error ?? "Sorgu planlanamadı";
                await SaveQueryHistoryAsync(tenantId, userId, request.Query, null, response, tokensUsed, stopwatch.ElapsedMilliseconds);
                return response;
            }

            queryPlan = planResult.Plan;
            response.Confidence = queryPlan.Confidence;
            response.Intent = queryPlan.Intent;

            // 4. Query plan doğrulama
            var planValidation = await _queryPlanValidator.ValidateAsync(
                queryPlan, tenantId, tenantValidation.Tier, ct);

            if (!planValidation.IsValid)
            {
                response.IsSuccess = false;
                response.ErrorCode = "VALIDATION_FAILED";
                response.ErrorMessage = string.Join("; ", planValidation.Errors);
                response.Suggestions = queryPlan.SuggestedCapabilities;
                
                if (planValidation.RestrictedCapabilities.Any())
                {
                    response.RequiresUpgrade = true;
                    response.UpgradeMessage = $"Bu sorgu için şu capability'lere erişim gerekli: {string.Join(", ", planValidation.RestrictedCapabilities)}";
                }

                await SaveQueryHistoryAsync(tenantId, userId, request.Query, queryPlan, response, tokensUsed, stopwatch.ElapsedMilliseconds);
                return response;
            }

            // 5. OutOfScope kontrolü
            if (queryPlan.Intent == QueryIntent.OutOfScope)
            {
                response.IsSuccess = false;
                response.ErrorCode = "OUT_OF_SCOPE";
                response.ErrorMessage = "Bu sorgu iş zekası kapsamı dışında";
                response.Suggestions = queryPlan.SuggestedCapabilities;
                await SaveQueryHistoryAsync(tenantId, userId, request.Query, queryPlan, response, tokensUsed, stopwatch.ElapsedMilliseconds);
                return response;
            }

            // 6. Capability'leri çalıştır
            var orchestrationResult = await _queryOrchestrator.ExecuteAsync(queryPlan, tenantId, ct);

            response.IsSuccess = orchestrationResult.Success;
            response.Results = orchestrationResult.Results;
            response.TotalExecutionTimeMs = orchestrationResult.TotalExecutionTimeMs;

            if (!orchestrationResult.Success)
            {
                response.ErrorCode = "EXECUTION_FAILED";
                response.ErrorMessage = orchestrationResult.Error;
            }

            // 7. Kota güncelle
            await _subscriptionValidator.ConsumeQuotaAsync(tenantId, tokensUsed, ct);

            // 8. Geçmişe kaydet
            stopwatch.Stop();
            await SaveQueryHistoryAsync(tenantId, userId, request.Query, queryPlan, response, tokensUsed, stopwatch.ElapsedMilliseconds);

            response.TotalExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            response.RemainingDailyQuota = quotaCheck.RemainingQueries - 1;

            _logger.LogInformation(
                "Query processed for tenant {TenantId}. Success: {Success}, Intent: {Intent}, Capabilities: {Count}, Time: {Time}ms",
                tenantId, response.IsSuccess, queryPlan.Intent, queryPlan.Capabilities.Count, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query processing failed for tenant {TenantId}", tenantId);

            response.IsSuccess = false;
            response.ErrorCode = "INTERNAL_ERROR";
            response.ErrorMessage = "Beklenmeyen bir hata oluştu";
            response.TotalExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            await SaveQueryHistoryAsync(tenantId, userId, request.Query, queryPlan, response, tokensUsed, stopwatch.ElapsedMilliseconds);

            return response;
        }
    }

    public Task<List<CapabilityInfoDto>> GetAvailableCapabilitiesAsync(
        int tenantId,
        CancellationToken ct = default)
    {
        // Tenant'ın tier'ını al ve uygun capability'leri döndür
        var tenant = _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.SubscriptionPlan)
            .FirstOrDefault();

        var tier = tenant?.Tier ?? SubscriptionTier.Free;
        var tierName = tier.ToString();
        var capabilities = _capabilityRegistry.GetCapabilitiesForTier(tierName);

        var result = capabilities.Select(c => c.GetInfo()).ToList();
        return Task.FromResult(result);
    }

    public async Task<List<QueryHistorySummaryDto>> GetQueryHistoryAsync(
        int tenantId,
        int? userId = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        var query = _dbContext.QueryHistories
            .Where(h => h.TenantId == tenantId);

        if (userId.HasValue)
        {
            query = query.Where(h => h.UserId == userId.Value);
        }

        var history = await Task.Run(() => query
            .OrderByDescending(h => h.CreatedAt)
            .Take(limit)
            .Select(h => new QueryHistorySummaryDto
            {
                Id = h.Id,
                Query = h.RawQuery,
                Intent = h.Intent.ToString(),
                Success = h.IsSuccess,
                CreatedAt = h.CreatedAt,
                ExecutionTimeMs = h.ExecutionTimeMs,
                TokensUsed = h.TokensUsed
            })
            .ToList(), ct);

        return history;
    }

    private async Task SaveQueryHistoryAsync(
        int tenantId,
        int userId,
        string query,
        QueryPlanDto? plan,
        BusinessQueryResponse response,
        int tokensUsed,
        long executionTimeMs)
    {
        try
        {
            var history = new QueryHistory
            {
                TenantId = tenantId,
                UserId = userId,
                RawQuery = query,
                Intent = plan?.Intent ?? QueryIntent.OutOfScope,
                Confidence = plan?.Confidence ?? 0,
                ParsedPlanJson = plan != null ? JsonSerializer.Serialize(plan, JsonOptions) : null,
                ResponseSummary = response.Summary,
                IsSuccess = response.IsSuccess,
                ErrorMessage = response.ErrorMessage,
                TokensUsed = tokensUsed,
                ExecutionTimeMs = executionTimeMs,
                DataSource = response.DataSource,
                ExecutedCapabilities = plan?.Capabilities != null 
                    ? string.Join(",", plan.Capabilities.Select(c => c.Name))
                    : null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.QueryHistories.Add(history);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save query history for tenant {TenantId}", tenantId);
            // Geçmiş kaydı başarısız olsa bile ana işlemi etkilemesin
        }
    }
}
