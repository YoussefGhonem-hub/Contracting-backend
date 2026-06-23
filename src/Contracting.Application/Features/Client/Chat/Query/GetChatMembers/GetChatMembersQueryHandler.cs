using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatMembers;

public class GetChatMembersQueryHandler : IRequestHandler<GetChatMembersQuery, ErrorOr<List<GetChatMemberDto>>>
{
    private readonly IChatService _chatService;

    public GetChatMembersQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<List<GetChatMemberDto>>> Handle(GetChatMembersQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetMembersAsync(request.GroupId, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.GroupNotFound", "Chat group not found or you are not a member.");

        return result;
    }
}
