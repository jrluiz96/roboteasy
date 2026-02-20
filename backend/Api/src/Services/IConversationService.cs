using Api.Contracts.Responses;

namespace Api.Services;

public interface IConversationService
{
    Task<List<ConversationListItemResponse>> GetActiveAsync(int userId);
    Task<List<ConversationListItemResponse>> GetHistoryAsync();
    Task<ConversationDetailResponse?> GetByIdAsync(long id);
    Task<bool> FinishAsync(long id);
    Task<bool> JoinAsync(long id, int userId);
    Task<bool> InviteAttendantAsync(long id, int invitedUserId);
    Task<bool> LeaveAsync(long id, int userId);
}
