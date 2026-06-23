using Contracting.Infrustructure.Inteface.client;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, ErrorOr<int>>
{
    private readonly IChatService _chatService;

    public GetUnreadCountQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetUnreadCountAsync(request.GroupId, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result.Value;
    }
}
