using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatMessages;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, ErrorOr<GetChatMessagesPagedDto>>
{
    private readonly IChatService _chatService;

    public GetChatMessagesQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<GetChatMessagesPagedDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetMessagesAsync(request.GroupId, request.Page, request.PageSize, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result;
    }
}
