using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/conversations")]
[Authorize]
[Tags("Conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _service;

    public ConversationsController(IConversationService service)
    {
        _service = service;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? throw new UnauthorizedAccessException());

    /// <summary>Lista conversas visíveis ao atendente (waiting = todas, active = só as vinculadas)</summary>
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveAsync(GetUserId());
        var response = ApiResponse<List<ConversationListItemResponse>>.Success(items, "Conversas ativas");
        return StatusCode(response.Code, response);
    }

    /// <summary>Lista conversas finalizadas (aba Histórico)</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var items = await _service.GetHistoryAsync();
        var response = ApiResponse<List<ConversationListItemResponse>>.Success(items, "Histórico de conversas");
        return StatusCode(response.Code, response);
    }

    /// <summary>Retorna uma conversa com mensagens completas e atendentes vinculados</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var detail = await _service.GetByIdAsync(id);
        if (detail == null)
        {
            var err = ApiResponse<object>.NotFound("Conversa não encontrada");
            return StatusCode(err.Code, err);
        }

        var response = ApiResponse<ConversationDetailResponse>.Success(detail, "Conversa carregada");
        return StatusCode(response.Code, response);
    }

    /// <summary>Atendente puxa a conversa para si</summary>
    [HttpPost("{id:long}/join")]
    public async Task<IActionResult> Join(long id)
    {
        var ok = await _service.JoinAsync(id, GetUserId());
        if (!ok)
        {
            var err = ApiResponse<object>.NotFound("Conversa não encontrada ou já finalizada");
            return StatusCode(err.Code, err);
        }

        var response = ApiResponse<object>.Success(null, "Conversa puxada com sucesso");
        return StatusCode(response.Code, response);
    }

    /// <summary>Convida outro atendente para participar da conversa</summary>
    [HttpPost("{id:long}/invite/{attendantId:int}")]
    public async Task<IActionResult> InviteAttendant(long id, int attendantId)
    {
        var ok = await _service.InviteAttendantAsync(id, attendantId);
        if (!ok)
        {
            var err = ApiResponse<object>.NotFound("Conversa não encontrada ou já finalizada");
            return StatusCode(err.Code, err);
        }

        var response = ApiResponse<object>.Success(null, "Atendente convidado com sucesso");
        return StatusCode(response.Code, response);
    }

    /// <summary>Atendente sai da conversa sem finalizá-la</summary>
    [HttpPost("{id:long}/leave")]
    public async Task<IActionResult> Leave(long id)
    {
        var ok = await _service.LeaveAsync(id, GetUserId());
        if (!ok)
        {
            var err = ApiResponse<object>.NotFound("Participação não encontrada");
            return StatusCode(err.Code, err);
        }

        var response = ApiResponse<object>.Success(null, "Você saiu da conversa");
        return StatusCode(response.Code, response);
    }

    /// <summary>Encerra uma conversa</summary>
    [HttpPost("{id:long}/finish")]
    public async Task<IActionResult> Finish(long id)
    {
        var ok = await _service.FinishAsync(id);
        if (!ok)
        {
            var err = ApiResponse<object>.NotFound("Conversa não encontrada ou já finalizada");
            return StatusCode(err.Code, err);
        }

        var response = ApiResponse<object>.Success(null, "Conversa finalizada");
        return StatusCode(response.Code, response);
    }
}

