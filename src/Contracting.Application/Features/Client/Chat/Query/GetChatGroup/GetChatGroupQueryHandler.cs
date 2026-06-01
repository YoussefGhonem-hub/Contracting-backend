using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatGroup;

public class GetChatGroupQueryHandler : IRequestHandler<GetChatGroupQuery, ErrorOr<GetChatGroupDto>>
{
    private readonly IChatService _chatService;

    public GetChatGroupQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<GetChatGroupDto>> Handle(GetChatGroupQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetGroupAsync(request.GroupId, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result;
    }
}
