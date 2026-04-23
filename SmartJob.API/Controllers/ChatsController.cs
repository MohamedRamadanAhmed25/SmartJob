using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Chats;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages real-time chats between employers and job seekers.
/// </summary>
[Authorize]
[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ICurrentUserService _currentUserService;

    public ChatsController(IChatService chatService, ICurrentUserService currentUserService)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a list of chats for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of chats.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ChatDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatDto>>> GetMyChats(CancellationToken cancellationToken)
    {
        return Ok(await _chatService.GetMyChatsAsync(_currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Gets an existing chat or creates a new one with the specified user.
    /// </summary>
    /// <param name="request">The request containing the recipient ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)] 
    public async Task<ActionResult<ChatDto>> GetOrCreateChat(CreateChatRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _chatService.GetOrCreateChatAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken));
    }

    /// <summary>
    /// Retrieves messages for a specific chat.
    /// </summary>
    /// <param name="id">The chat ID.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of messages.</returns>
    [HttpGet("{id}/messages")]
    [ProducesResponseType(typeof(PagedMessages), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedMessages>> GetMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return Ok(await _chatService.GetMessagesAsync(id, _currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Sends a message in a specific chat.
    /// </summary>
    /// <param name="id">The chat ID.</param>
    /// <param name="request">The message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created message.</returns>
    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MessageDto>> SendMessage(Guid id, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var msg = await _chatService.SendMessageAsync(id, _currentUserService.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetMessages), new { id = id }, msg);
    }

    /// <summary>
    /// Marks all messages as read for a specific chat for the current user.
    /// </summary>
    /// <param name="id">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkChatAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _chatService.MarkChatAsReadAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }
}
