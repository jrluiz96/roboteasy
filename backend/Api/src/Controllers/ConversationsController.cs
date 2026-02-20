using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>Lista conversas abertas (aba Chats do atendente)</summary>
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveAsync();
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

    /// <summary>Retorna uma conversa com mensagens completas</summary>
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
