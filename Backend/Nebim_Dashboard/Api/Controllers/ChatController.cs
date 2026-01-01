using Api.Common;
using BLL.Services.Interfaces;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// AI Chat controller'ı
/// </summary>
[Authorize]
public class ChatController : BaseController
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// Yeni sohbet oturumu başlat
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ApiResponse<ChatSessionResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ChatSessionResponse>>> StartSession(
        [FromBody] StartSessionRequest? request,
        CancellationToken ct)
    {
        var session = await _chatService.StartSessionAsync(
            CurrentUserId, 
            request?.Title, 
            ct);
        
        return CreatedAtAction(
            nameof(GetSession),
            new { sessionId = session.Id },
            ApiResponse<ChatSessionResponse>.Success(session, "Oturum başlatıldı"));
    }

    /// <summary>
    /// Mesaj gönder
    /// </summary>
    [HttpPost("sessions/{sessionId}/messages")]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ChatMessageResponse>>> SendMessage(
        int sessionId,
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        var response = await _chatService.SendMessageAsync(sessionId, request, ct);
        return Ok(ApiResponse<ChatMessageResponse>.Success(response));
    }

    /// <summary>
    /// Oturum detayı (mesaj geçmişi ile)
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<ChatSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ChatSessionResponse>>> GetSession(
        int sessionId,
        CancellationToken ct)
    {
        var session = await _chatService.GetSessionAsync(sessionId, ct);
        
        if (session == null)
            return NotFound(ApiErrorResponse.Create($"Oturum bulunamadı: {sessionId}"));
        
        return Ok(ApiResponse<ChatSessionResponse>.Success(session));
    }

    /// <summary>
    /// Kullanıcının tüm oturumları
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<List<ChatSessionSummary>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ChatSessionSummary>>>> GetMySessions(
        CancellationToken ct)
    {
        var sessions = await _chatService.GetUserSessionsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<List<ChatSessionSummary>>.Success(sessions));
    }

    /// <summary>
    /// Oturumu sil
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(int sessionId, CancellationToken ct)
    {
        var deleted = await _chatService.DeleteSessionAsync(sessionId, ct);
        
        if (!deleted)
            return NotFound(ApiErrorResponse.Create($"Oturum bulunamadı: {sessionId}"));
        
        return NoContent();
    }
}

/// <summary>
/// Oturum başlatma request'i
/// </summary>
public class StartSessionRequest
{
    public string? Title { get; set; }
}
