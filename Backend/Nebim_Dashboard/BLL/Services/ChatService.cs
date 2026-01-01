using BLL.Mappings;
using BLL.Services.Interfaces;
using DAL.UnitOfWork;
using Entity.App;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Entity.Enums;
using Entity.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Chat servisi
/// 
/// 🎓 AÇIKLAMA:
/// - AI sohbet oturumlarını yönetir
/// - Mesaj geçmişini saklar ve getirir
/// - Google Gemini 1.5 ile AI yanıtı üretir
/// </summary>
public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatService> _logger;
    private readonly IAIService _aiService;
    
    public ChatService(
        IUnitOfWork unitOfWork,
        ILogger<ChatService> logger,
        IAIService aiService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _aiService = aiService;
    }
    
    /// <summary>
    /// Yeni sohbet oturumu başlat
    /// </summary>
    public async Task<ChatSessionResponse> StartSessionAsync(
        int userId, 
        string? title = null, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Yeni chat oturumu başlatılıyor. User: {UserId}", userId);
        
        // Kullanıcı kontrolü
        var userExists = await _unitOfWork.Repository<User>().AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {userId}");
        }
        
        var session = new ChatSession
        {
            UserId = userId,
            Title = title ?? $"Sohbet - {DateTime.Now:dd.MM.yyyy HH:mm}",
            IsActive = true
        };
        
        await _unitOfWork.Repository<ChatSession>().AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Chat oturumu oluşturuldu: {SessionId}", session.Id);
        
        return session.ToResponse(includeMessages: false);
    }
    
    /// <summary>
    /// Mesaj gönder ve AI yanıtı al
    /// </summary>
    public async Task<ChatMessageResponse> SendMessageAsync(
        int sessionId, 
        ChatRequest request, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Mesaj gönderiliyor. Session: {SessionId}", sessionId);
        
        // 1. Oturumu bul
        var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(sessionId, ct);
        
        if (session == null)
        {
            throw new NotFoundException($"Oturum bulunamadı: {sessionId}");
        }
        
        if (!session.IsActive)
        {
            throw new BusinessException("Bu oturum artık aktif değil");
        }
        
        // 2. Kullanıcı mesajını kaydet
        var userMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = ChatRole.User,
            Content = request.Message,
            Type = MessageType.Text
        };
        
        await _unitOfWork.Repository<ChatMessage>().AddAsync(userMessage, ct);
        
        // 3. Mesaj geçmişini al (context için)
        var previousMessages = await _unitOfWork.Repository<ChatMessage>()
            .Query()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageResponse
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                Type = m.Type,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);
        
        // 4. Google Gemini ile AI yanıtı üret
        var aiResponse = await _aiService.GenerateResponseAsync(request.Message, previousMessages, ct);
        
        var aiMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = ChatRole.Assistant,
            Content = aiResponse,
            Type = MessageType.Text
        };
        
        await _unitOfWork.Repository<ChatMessage>().AddAsync(aiMessage, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Mesaj ve AI yanıtı kaydedildi. Session: {SessionId}", sessionId);
        
        return aiMessage.ToResponse();
    }
    
    /// <summary>
    /// Oturum geçmişini getir
    /// </summary>
    public async Task<ChatSessionResponse?> GetSessionAsync(int sessionId, CancellationToken ct = default)
    {
        _logger.LogDebug("Oturum getiriliyor: {SessionId}", sessionId);
        
        // 🎓 Include: İlişkili verileri de getir (Eager Loading)
        var session = await _unitOfWork.Repository<ChatSession>()
            .Query()
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        
        return session?.ToResponse(includeMessages: true);
    }
    
    /// <summary>
    /// Kullanıcının tüm oturumlarını getir
    /// </summary>
    public async Task<List<ChatSessionSummary>> GetUserSessionsAsync(int userId, CancellationToken ct = default)
    {
        _logger.LogDebug("Kullanıcı oturumları getiriliyor: {UserId}", userId);
        
        var sessions = await _unitOfWork.Repository<ChatSession>()
            .Query()
            .Include(s => s.Messages)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
        
        return sessions.Select(s => s.ToSummary()).ToList();
    }
    
    /// <summary>
    /// Oturumu sil
    /// </summary>
    public async Task<bool> DeleteSessionAsync(int sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Oturum siliniyor: {SessionId}", sessionId);
        
        var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(sessionId, ct);
        
        if (session == null)
            return false;
        
        // 🎓 Soft delete yerine hard delete (chat geçmişi için)
        _unitOfWork.Repository<ChatSession>().Delete(session);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return true;
    }
}
