using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatTabMedia;

public class GetChatMediaQueryHandler : IRequestHandler<GetChatMediaQuery, ErrorOr<List<GetChatMessageDto>>>
{
    private readonly IChatService _chatService;

    public GetChatMediaQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<List<GetChatMessageDto>>> Handle(GetChatMediaQuery request, CancellationToken cancellationToken)
    {
        List<GetChatMessageDto>? result = request.Tab.ToLower() switch
        {
            "media" => await _chatService.GetMediaMessagesAsync(request.GroupId, cancellationToken),
            "docs" => await _chatService.GetDocumentMessagesAsync(request.GroupId, cancellationToken),
            "links" => await _chatService.GetLinkMessagesAsync(request.GroupId, cancellationToken),
            _ => null
        };

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result;
    }
}
