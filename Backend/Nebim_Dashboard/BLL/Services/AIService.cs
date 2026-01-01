using BLL.Services.Interfaces;
using DAL.Repositories;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Entity.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace BLL.Services;

/// <summary>
/// Google Gemini AI Servisi - Direct HTTP Client
/// SK yerine doğrudan REST API kullanır - daha stabil ve verimli
/// </summary>
public class AIService : IAIService
{
    private readonly ILogger<AIService> _logger;
    private readonly INebimRepository _nebimRepository;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;
    
    // System prompt - statik, systemInstruction olarak gönderilir (her mesajda tekrar etmez)
    private static readonly string SystemInstruction = """
        Sen Nebim ERP için Türkçe konuşan perakende asistanısın.
        Kısa, net yanıtlar ver. TL kullan. Emoji az kullan.
        Stok, satış, ürün ve müşteri sorularını yanıtla.
        """;
    
    public AIService(
        IConfiguration configuration,
        ILogger<AIService> logger,
        INebimRepository nebimRepository)
    {
        _logger = logger;
        _nebimRepository = nebimRepository;
        
        _apiKey = configuration["AI:ApiKey"] 
            ?? throw new InvalidOperationException("AI:ApiKey yapılandırması bulunamadı");
        _model = configuration["AI:Model"] ?? "gemini-2.0-flash";
        _baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        
        _logger.LogInformation("Gemini AI servisi başlatıldı. Model: {Model}", _model);
    }
    
    /// <inheritdoc/>
    public async Task<string> GenerateResponseAsync(
        string userMessage, 
        List<ChatMessageResponse>? chatHistory = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("AI isteği: {Message}", userMessage.Length > 100 ? userMessage[..100] + "..." : userMessage);
            
            // Veri zenginleştirme
            var enrichedMessage = await EnrichPromptWithDataAsync(userMessage, ct);
            
            // Request body oluştur
            var request = BuildRequest(enrichedMessage, chatHistory);
            
            // API çağrısı (retry ile)
            var url = $"{_baseUrl}?key={_apiKey}";
            
            HttpResponseMessage? response = null;
            int maxRetries = 3;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                response = await _httpClient.PostAsJsonAsync(url, request, ct);
                
                // Rate limit ise bekle ve tekrar dene
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < maxRetries)
                {
                    var waitSeconds = attempt * 5; // 5, 10, 15 saniye
                    _logger.LogWarning("Rate limit aşıldı. {Attempt}. deneme, {Wait} saniye bekleniyor...", attempt, waitSeconds);
                    await Task.Delay(waitSeconds * 1000, ct);
                    continue;
                }
                
                break;
            }
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                var error = response != null ? await response.Content.ReadAsStringAsync(ct) : "No response";
                var statusCode = response?.StatusCode ?? System.Net.HttpStatusCode.ServiceUnavailable;
                _logger.LogError("Gemini API hatası: {Status} - {Error}", statusCode, error);
                
                return statusCode switch
                {
                    System.Net.HttpStatusCode.TooManyRequests => "⚠️ AI servisi şu an yoğun. Lütfen 1 dakika sonra tekrar deneyin.",
                    System.Net.HttpStatusCode.NotFound => "❌ AI model yapılandırması hatalı.",
                    System.Net.HttpStatusCode.Unauthorized => "🔑 API anahtarı geçersiz.",
                    _ => $"AI servisi yanıt veremedi: {statusCode}"
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(ct);
            var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            
            if (string.IsNullOrEmpty(text))
            {
                _logger.LogWarning("Gemini boş yanıt döndü");
                return "Üzgünüm, yanıt üretilemedi.";
            }
            
            _logger.LogDebug("AI yanıtı alındı. Uzunluk: {Length}", text.Length);
            return text;
        }
        catch (TaskCanceledException)
        {
            return "⏱️ İstek zaman aşımına uğradı. Lütfen tekrar deneyin.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI hatası");
            return $"Hata: {ex.Message}";
        }
    }
    
    private object BuildRequest(string userMessage, List<ChatMessageResponse>? history)
    {
        var contents = new List<object>();
        
        // Sadece son 5 mesajı ekle (context tasarrufu)
        if (history?.Count > 0)
        {
            foreach (var msg in history.TakeLast(5))
            {
                contents.Add(new
                {
                    role = msg.Role == ChatRole.User ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }
        }
        
        // Mevcut mesaj
        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = userMessage } }
        });
        
        return new
        {
            contents,
            systemInstruction = new
            {
                parts = new[] { new { text = SystemInstruction } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 1024,
                topP = 0.9
            }
        };
    }
    
    /// <inheritdoc/>
    public async Task<string> AskAboutStockAsync(string question, CancellationToken ct = default)
    {
        var stockData = await GetStockContextAsync(ct);
        var prompt = $"STOK VERİLERİ:\n{stockData}\n\nSORU: {question}";
        return await GenerateResponseAsync(prompt, null, ct);
    }
    
    /// <inheritdoc/>
    public async Task<string> AskAboutSalesAsync(string question, CancellationToken ct = default)
    {
        var salesData = await GetDetailedSalesContextAsync(ct);
        var prompt = $"SATIŞ VERİLERİ:\n{salesData}\n\nSORU: {question}";
        return await GenerateResponseAsync(prompt, null, ct);
    }
    
    #region Private Methods
    
    private async Task<string> EnrichPromptWithDataAsync(string userMessage, CancellationToken ct)
    {
        var lower = userMessage.ToLower();
        var sb = new StringBuilder();
        
        // Her zaman temel verileri ekle - AI gerçek verilerle çalışsın
        var needsData = ContainsAny(lower, "stok", "envanter", "adet", "depo", "tüken", 
            "satış", "ciro", "gelir", "satılan", "sipariş", "en çok", "kaç", "ne kadar",
            "ürün", "kategori", "marka", "fiyat", "tekstil", "giyim", "ayakkabı",
            "bugün", "hafta", "ay", "rapor", "analiz", "durum");
        
        if (needsData)
        {
            sb.AppendLine("=== NEBIM VERİLERİ ===");
            sb.AppendLine();
            
            // Satış verileri
            sb.AppendLine("📊 SATIŞ VERİLERİ:");
            sb.AppendLine(await GetDetailedSalesContextAsync(ct));
            sb.AppendLine();
            
            // En çok satanlar
            sb.AppendLine("🏆 EN ÇOK SATAN ÜRÜNLER:");
            sb.AppendLine(await GetTopSellingProductsAsync(ct));
            sb.AppendLine();
            
            // Stok durumu
            sb.AppendLine("📦 STOK DURUMU:");
            sb.AppendLine(await GetStockContextAsync(ct));
            sb.AppendLine();
            
            sb.AppendLine("=== KULLANICI SORUSU ===");
            sb.AppendLine(userMessage);
            sb.AppendLine();
            sb.AppendLine("Yukarıdaki GERÇEK verilere dayanarak yanıt ver. Tahmini bilgi verme.");
            
            return sb.ToString();
        }
        
        return userMessage;
    }
    
    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    
    private async Task<string> GetStockContextAsync(CancellationToken ct)
    {
        try
        {
            // Önce özet bilgileri al (tüm veriden)
            var (totalRecords, totalQuantity) = await _nebimRepository.GetStockSummaryAsync(ct);
            
            // Düşük stoklu ürünleri al
            var lowStockItems = await _nebimRepository.GetLowStockItemsAsync(10, ct);
            
            var sb = new StringBuilder();
            sb.AppendLine($"- Toplam stok kaydı: {totalRecords} adet kayıt");
            sb.AppendLine($"- Toplam stok miktarı: {totalQuantity} adet ürün");
            sb.AppendLine($"- Düşük stoklu ürün sayısı: {lowStockItems.Count}");
            
            // Düşük stoktaki ürünleri listele
            if (lowStockItems.Any())
            {
                sb.AppendLine("- Kritik stok ürünler:");
                foreach (var item in lowStockItems.Take(10))
                {
                    sb.AppendLine($"  • {item.ProductName}: {item.Quantity} adet ({item.WarehouseName})");
                }
            }
            
            return sb.ToString();
        }
        catch (Exception ex) 
        { 
            _logger.LogWarning(ex, "Stok verisi alınamadı");
            return "Stok verisi alınamadı"; 
        }
    }
    
    private async Task<string> GetDetailedSalesContextAsync(CancellationToken ct)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30); // Frontend ile aynı: son 30 gün
            
            var filter = new SalesFilterRequest 
            { 
                Page = 1, 
                PageSize = 50,
                StartDate = startDate,
                EndDate = endDate
            };
            var result = await _nebimRepository.GetSalesAsync(filter, ct);
            var sales = result.Items;
            
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalItems = sales.Sum(s => s.Quantity);
            var avgOrder = sales.Count > 0 ? sales.Average(s => s.TotalAmount) : 0;
            
            var sb = new StringBuilder();
            sb.AppendLine($"- Son 7 gün toplam ciro: {totalRevenue:N0} TL");
            sb.AppendLine($"- Satış adedi: {sales.Count}");
            sb.AppendLine($"- Satılan ürün: {totalItems} adet");
            sb.AppendLine($"- Ortalama sepet: {avgOrder:N0} TL");
            
            // Günlük dağılım
            var dailySales = sales.GroupBy(s => s.SaleDate.Date)
                .OrderByDescending(g => g.Key)
                .Take(7);
            
            sb.AppendLine("- Günlük satışlar:");
            foreach (var day in dailySales)
            {
                sb.AppendLine($"  • {day.Key:dd/MM}: {day.Sum(s => s.TotalAmount):N0} TL ({day.Count()} satış)");
            }
            
            return sb.ToString();
        }
        catch (Exception ex) 
        { 
            _logger.LogWarning(ex, "Satış verisi alınamadı");
            return "Satış verisi alınamadı"; 
        }
    }
    
    private async Task<string> GetTopSellingProductsAsync(CancellationToken ct)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30); // Frontend ile aynı: son 30 gün
            
            var topProducts = await _nebimRepository.GetTopSellingProductsAsync(startDate, endDate, 10, ct);
            
            if (!topProducts.Any())
                return "Bu dönemde satış verisi yok";
            
            var sb = new StringBuilder();
            var rank = 1;
            foreach (var product in topProducts)
            {
                sb.AppendLine($"{rank}. {product.ProductName} - {product.Quantity} adet - {product.TotalAmount:N0} TL");
                rank++;
            }
            
            return sb.ToString();
        }
        catch (Exception ex) 
        { 
            _logger.LogWarning(ex, "En çok satan ürünler alınamadı");
            return "En çok satan ürünler alınamadı"; 
        }
    }
    
    private async Task<string> GetProductContextAsync(CancellationToken ct)
    {
        try
        {
            var filter = new StockFilterRequest { Page = 1, PageSize = 20 };
            var result = await _nebimRepository.GetProductsAsync(filter, ct);
            var products = result.Items;
            var categories = products.Select(p => p.CategoryName).Distinct().Take(5);
            var brands = products.Select(p => p.BrandName).Distinct().Take(5);
            
            return $"Kategoriler: {string.Join(", ", categories)}, Markalar: {string.Join(", ", brands)}";
        }
        catch { return "Ürün verisi alınamadı"; }
    }
    
    #endregion
}

#region Gemini API Response Models

internal class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

internal class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

internal class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }
}

internal class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

#endregion
