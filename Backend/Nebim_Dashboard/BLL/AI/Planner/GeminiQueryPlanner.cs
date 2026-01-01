using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BLL.AI.Capabilities;
using Entity.DTOs.AI;
using Entity.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Planner;

/// <summary>
/// Google Gemini 2.0 Flash kullanan Query Planner implementasyonu.
/// Doğal dil sorgusunu JSON Query Plan'a çevirir.
/// </summary>
public class GeminiQueryPlanner : IQueryPlanner
{
    private readonly HttpClient _httpClient;
    private readonly ICapabilityRegistry _capabilityRegistry;
    private readonly ILogger<GeminiQueryPlanner> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiQueryPlanner(
        HttpClient httpClient,
        ICapabilityRegistry capabilityRegistry,
        IConfiguration configuration,
        ILogger<GeminiQueryPlanner> logger)
    {
        _httpClient = httpClient;
        _capabilityRegistry = capabilityRegistry;
        _logger = logger;

        _apiKey = configuration["Gemini:ApiKey"] 
            ?? throw new InvalidOperationException("Gemini:ApiKey configuration is required");
        _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
    }

    public async Task<QueryPlanResult> CreatePlanAsync(
        string query,
        int tenantId,
        SubscriptionTier subscriptionTier,
        List<ConversationMessage>? conversationHistory = null,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var systemPrompt = BuildSystemPrompt(subscriptionTier);
            var userPrompt = BuildUserPrompt(query, conversationHistory);

            var request = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new() { Role = "user", Parts = new List<GeminiPart> { new() { Text = systemPrompt + "\n\n" + userPrompt } } }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.1f, // Düşük temperature = daha deterministik
                    TopK = 40,
                    TopP = 0.95f,
                    MaxOutputTokens = 2048,
                    ResponseMimeType = "application/json"
                }
            };

            var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";
            var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                return new QueryPlanResult
                {
                    Success = false,
                    Error = $"AI service error: {response.StatusCode}",
                    RawResponse = responseBody,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody, JsonOptions);
            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(text))
            {
                return new QueryPlanResult
                {
                    Success = false,
                    Error = "AI returned empty response",
                    RawResponse = responseBody,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            // JSON parse
            var plan = ParseQueryPlan(text, query);
            
            // Token usage
            var tokensUsed = geminiResponse?.UsageMetadata?.TotalTokenCount ?? 0;

            _logger.LogInformation(
                "Query plan created for tenant {TenantId}. Intent: {Intent}, Capabilities: {Count}, Tokens: {Tokens}, Latency: {Latency}ms",
                tenantId, plan.Intent, plan.Capabilities.Count, tokensUsed, stopwatch.ElapsedMilliseconds);

            return new QueryPlanResult
            {
                Success = true,
                Plan = plan,
                RawResponse = text,
                TokensUsed = tokensUsed,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to parse Gemini response for tenant {TenantId}", tenantId);
            return new QueryPlanResult
            {
                Success = false,
                Error = "Failed to parse AI response",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query planning failed for tenant {TenantId}", tenantId);
            return new QueryPlanResult
            {
                Success = false,
                Error = ex.Message,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public string GetCapabilityDescriptions()
    {
        return _capabilityRegistry.GetCapabilityDescriptionsForPrompt();
    }

    private string BuildSystemPrompt(SubscriptionTier tier)
    {
        var capabilities = _capabilityRegistry.GetCapabilityDescriptionsForPrompt();

        var jsonFormat = @"{
    ""intent"": ""Descriptive|Diagnostic|Comparative|Predictive|OutOfScope"",
    ""confidence"": 0.0-1.0,
    ""capabilities"": [
        {
            ""name"": ""CapabilityName"",
            ""version"": ""v1"",
            ""parameters"": {},
            ""order"": 1,
            ""dependsOn"": []
        }
    ],
    ""suggestedCapabilities"": [
        {
            ""name"": ""CapabilityName"",
            ""reason"": ""Neden önerildiği"",
            ""confidence"": 0.0-1.0
        }
    ]
}";

        return $"""
        Sen bir iş zekası sorgu planlayıcısısın. Kullanıcının Türkçe veya İngilizce sorularını analiz edip, 
        hangi capability'lerin çağrılması gerektiğini belirleyeceksin.

        ÖNEMLİ KURALLAR:
        1. SADECE JSON formatında cevap ver, başka hiçbir şey yazma
        2. Cevap oluşturma, yorum yapma, bilgi uydurma - sadece sorgu planı döndür
        3. Eğer sorgu iş zekası ile ilgili değilse, boş capabilities listesi döndür
        4. Tarih aralıkları için makul varsayılanlar kullan (örn: "bu ay" = ayın başından bugüne)
        5. Belirsiz durumlarda confidence düşük olsun ve suggestions ekle

        MEVCUT CAPABILITY'LER:
        {capabilities}

        JSON FORMATI:
        {jsonFormat}

        INTENT AÇIKLAMALARI:
        - Query: Veri sorgulama (satışlar, stok, ürünler vb.)
        - Command: Bir işlem yapma talebi (not: şu an desteklenmiyor)
        - Clarification: Belirsiz sorgu, açıklama gerekiyor
        - OutOfScope: İş zekası dışı sorgu

        ÖNEMLİ: Türkçe tarih ifadelerini doğru anla:
        - "bugün" = DateTime.Today
        - "dün" = DateTime.Today.AddDays(-1)
        - "bu hafta" = Haftanın başı - bugün
        - "bu ay" = Ayın başı - bugün
        - "geçen ay" = Önceki ayın başı - sonu
        - "son 7 gün" = Bugünden 7 gün önce - bugün
        """;
    }

    private string BuildUserPrompt(string query, List<ConversationMessage>? conversationHistory)
    {
        var sb = new StringBuilder();

        if (conversationHistory?.Any() == true)
        {
            sb.AppendLine("ÖNCEKİ KONUŞMA:");
            foreach (var msg in conversationHistory.TakeLast(5))
            {
                sb.AppendLine($"[{msg.Role}]: {msg.Content}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"KULLANICI SORGUSU: {query}");
        sb.AppendLine();
        sb.AppendLine("Bu sorguyu analiz et ve JSON formatında sorgu planı döndür.");

        return sb.ToString();
    }

    private QueryPlanDto ParseQueryPlan(string jsonText, string originalQuery)
    {
        try
        {
            // JSON text'ten boşlukları ve markdown işaretlerini temizle
            var cleanJson = jsonText.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            var parsed = JsonSerializer.Deserialize<GeminiQueryPlanResponse>(cleanJson, JsonOptions);

            if (parsed == null)
            {
                return CreateOutOfScopePlan(originalQuery, "AI response could not be parsed");
            }

            // Intent enum'a çevir
            var intent = parsed.Intent?.ToLowerInvariant() switch
            {
                "descriptive" => QueryIntent.Descriptive,
                "diagnostic" => QueryIntent.Diagnostic,
                "comparative" => QueryIntent.Comparative,
                "predictive" => QueryIntent.Predictive,
                "query" => QueryIntent.Descriptive, // Legacy support
                _ => QueryIntent.OutOfScope
            };

            return new QueryPlanDto
            {
                OriginalQuery = originalQuery,
                Intent = intent,
                Confidence = parsed.Confidence ?? 0.5,
                Capabilities = parsed.Capabilities?.Select((c, index) => new CapabilityCallDto
                {
                    Name = c.Name ?? string.Empty,
                    Version = c.Version ?? "v1",
                    Parameters = c.Parameters,
                    Order = c.Priority ?? index,
                    DependsOn = c.DependsOn ?? new List<string>()
                }).ToList() ?? new List<CapabilityCallDto>(),
                SuggestedCapabilities = parsed.SuggestedCapabilities?.Select(s => new SuggestedCapabilityDto
                {
                    Name = s.Name ?? string.Empty,
                    Description = s.Reason ?? string.Empty,
                    RelevanceScore = s.Confidence ?? 0.5,
                    ExampleQuery = string.Empty
                }).ToList()
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse query plan JSON: {Json}", jsonText);
            return CreateOutOfScopePlan(originalQuery, "Failed to parse AI response");
        }
    }

    private QueryPlanDto CreateOutOfScopePlan(string query, string reason)
    {
        return new QueryPlanDto
        {
            OriginalQuery = query,
            Intent = QueryIntent.OutOfScope,
            Confidence = 0.0,
            Capabilities = new List<CapabilityCallDto>(),
            SuggestedCapabilities = _capabilityRegistry.GetAllCapabilities()
                .Take(3)
                .Select(c => new SuggestedCapabilityDto
                {
                    Name = c.Name,
                    Description = c.Description,
                    RelevanceScore = 0.3,
                    ExampleQuery = c.ExampleQueries.FirstOrDefault() ?? string.Empty
                }).ToList()
        };
    }

    #region Gemini API Models

    private class GeminiRequest
    {
        public List<GeminiContent> Contents { get; set; } = new();
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        public string Role { get; set; } = string.Empty;
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiGenerationConfig
    {
        public float Temperature { get; set; }
        public int TopK { get; set; }
        public float TopP { get; set; }
        public int MaxOutputTokens { get; set; }
        public string? ResponseMimeType { get; set; }
    }

    private class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private class GeminiUsageMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    private class GeminiQueryPlanResponse
    {
        public string? Intent { get; set; }
        public double? Confidence { get; set; }
        public List<GeminiCapabilityCall>? Capabilities { get; set; }
        public List<GeminiSuggestedCapability>? SuggestedCapabilities { get; set; }
    }

    private class GeminiCapabilityCall
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public JsonElement? Parameters { get; set; }
        public int? Priority { get; set; }
        public List<string>? DependsOn { get; set; }
    }

    private class GeminiSuggestedCapability
    {
        public string? Name { get; set; }
        public string? Reason { get; set; }
        public double? Confidence { get; set; }
    }

    #endregion
}
