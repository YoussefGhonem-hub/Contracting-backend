using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetFirebaseToken;

public class GetFirebaseTokenQueryHandler : IRequestHandler<GetFirebaseTokenQuery, ErrorOr<FirebaseTokenDto>>
{
    private readonly IChatService _chatService;

    public GetFirebaseTokenQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<FirebaseTokenDto>> Handle(GetFirebaseTokenQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetFirebaseTokenAsync(request.GroupId, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result;
    }
}
