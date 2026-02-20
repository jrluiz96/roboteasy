using Api.Contracts.Responses;

namespace Api.Services;

public interface IConversationService
{
    Task<List<ConversationListItemResponse>> GetActiveAsync();
    Task<List<ConversationListItemResponse>> GetHistoryAsync();
    Task<ConversationDetailResponse?> GetByIdAsync(long id);
    Task<bool> FinishAsync(long id);
}
